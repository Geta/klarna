using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using EPiServer.Logging;
using Klarna.Checkout.Models;
using Klarna.Common;
using Klarna.Common.Extensions;
using Klarna.Common.Helpers;
using Klarna.OrderManagement;
using Klarna.Payments.Models;
using Klarna.Rest;
using Klarna.Rest.Checkout;
using Klarna.Rest.Models;
using Klarna.Rest.Transport;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using ConfigurationException = EPiServer.Business.Commerce.Exception.ConfigurationException;

namespace Klarna.Checkout
{
    [ServiceConfiguration(typeof(IKlarnaCheckoutService))]
    public class KlarnaCheckoutService : KlarnaService, IKlarnaCheckoutService
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(KlarnaCheckoutService));
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IKlarnaOrderValidator _klarnaOrderValidator;
        private readonly IMarketService _marketService;
        private readonly ICheckoutConfigurationLoader _checkoutConfigurationLoader;
        private readonly KlarnaOrderServiceFactory _klarnaOrderServiceFactory;
        private readonly IOrderRepository _orderRepository;

        private Client _client;
        private PaymentMethodDto _paymentMethodDto;
        private CheckoutConfiguration _checkoutConfiguration;

        public KlarnaCheckoutService(
            IOrderRepository orderRepository,
            IPaymentProcessor paymentProcessor,
            IOrderGroupCalculator orderGroupCalculator,
            IKlarnaOrderValidator klarnaOrderValidator,
            IMarketService marketService,
            ICheckoutConfigurationLoader checkoutConfigurationLoader,
            KlarnaOrderServiceFactory klarnaOrderServiceFactory)
            : base(orderRepository, paymentProcessor, orderGroupCalculator, marketService)
        {
            _orderGroupCalculator = orderGroupCalculator;
            _orderRepository = orderRepository;
            _klarnaOrderValidator = klarnaOrderValidator;
            _marketService = marketService;
            _checkoutConfigurationLoader = checkoutConfigurationLoader;
            _klarnaOrderServiceFactory = klarnaOrderServiceFactory;
        }

        public PaymentMethodDto PaymentMethodDto =>
            _paymentMethodDto
            ?? (_paymentMethodDto =
                PaymentManager.GetPaymentMethodBySystemName(
                    Constants.KlarnaCheckoutSystemKeyword, ContentLanguage.PreferredCulture.Name, returnInactive: true));

        public CheckoutConfiguration GetCheckoutConfiguration(IMarket market)
        {
            return _checkoutConfiguration ?? (_checkoutConfiguration = GetConfiguration(market.MarketId));
        }

        public Client GetClient(IMarket market)
        {
            if (PaymentMethodDto != null)
            {
                var connectionConfiguration = GetCheckoutConfiguration(market);
                var connector = ConnectorFactory.Create(connectionConfiguration.Username, connectionConfiguration.Password, new Uri(connectionConfiguration.ApiUrl));
                connector.UserAgent.AddField("Platform", "EPiServer", typeof(EPiServer.Core.IContent).Assembly.GetName().Version.ToString(), new string[0]);
                connector.UserAgent.AddField("Module", "Klarna.Checkout", typeof(KlarnaCheckoutService).Assembly.GetName().Version.ToString(), new string[0]);

                _client = new Client(connector);
            }
            return _client;
        }

        public CheckoutOrderData CreateOrUpdateOrder(ICart cart)
        {
            var orderId = cart.Properties[Constants.KlarnaCheckoutOrderIdCartField]?.ToString();
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return CreateOrder(cart);
            }
            else
            {
                return UpdateOrder(orderId, cart);
            }
        }

        public CheckoutOrderData CreateOrder(ICart cart)
        {
            var market = _marketService.GetMarket(cart.MarketId);
            var checkout = CreateCheckoutOrder(market);
            var orderData = GetCheckoutOrderData(cart, market, PaymentMethodDto);
            var checkoutConfiguration = GetCheckoutConfiguration(market);

            if (checkoutConfiguration.PrefillAddress)
            {
                // KCO_4: In case of signed in user the email address and default address details will be prepopulated by data from Merchant system.
                var customerContact = CustomerContext.Current.GetContactById(cart.CustomerId);
                if (customerContact?.PreferredBillingAddress != null)
                {
                    orderData.BillingAddress = customerContact.PreferredBillingAddress.ToAddress();
                }

                if (orderData.Options.AllowSeparateShippingAddress.HasValue &&
                    orderData.Options.AllowSeparateShippingAddress.Value)
                {
                    var shipment = cart.GetFirstShipment();
                    if (shipment.ShippingAddress != null)
                    {
                        orderData.ShippingAddress = shipment.ShippingAddress.ToAddress();
                    }
                }
            }
            try
            {
                if (ServiceLocator.Current.TryGetExistingInstance(out ICheckoutOrderDataBuilder checkoutOrderDataBuilder))
                {
                    checkoutOrderDataBuilder.Build(orderData, cart, checkoutConfiguration);
                }
                checkout.Create(orderData);

                orderData = checkout.Fetch();

                UpdateCartWithOrderData(cart, orderData);

                return orderData;
            }
            catch (ApiException ex)
            {
                _logger.Error($"{ex.ErrorMessage.CorrelationId} {ex.ErrorMessage.ErrorCode} {string.Join(", ", ex.ErrorMessage.ErrorMessages)}", ex);
                throw;
            }
            catch (WebException ex)
            {
                _logger.Error(ex.Message, ex);
                throw;
            }
        }

        public CheckoutOrderData UpdateOrder(string orderId, ICart cart)
        {
            var market = _marketService.GetMarket(cart.MarketId);
            var checkout = CreateCheckoutOrder(orderId, market);
            var orderData = GetCheckoutOrderData(cart, market, PaymentMethodDto);
            var checkoutConfiguration = GetCheckoutConfiguration(market);

            try
            {
                if (ServiceLocator.Current.TryGetExistingInstance(out ICheckoutOrderDataBuilder checkoutOrderDataBuilder))
                {
                    checkoutOrderDataBuilder.Build(orderData, cart, checkoutConfiguration);
                }

                orderData = checkout.Update(orderData);

                cart = UpdateCartWithOrderData(cart, orderData);

                return orderData;
            }
            catch (ApiException ex)
            {
                // Create new session if current one is not found
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return CreateOrder(cart);
                }

                _logger.Error($"{ex.ErrorMessage.CorrelationId} {ex.ErrorMessage.ErrorCode} {string.Join(", ", ex.ErrorMessage.ErrorMessages)}", ex);

                throw;
            }
            catch (WebException ex)
            {
                if ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                {
                    return CreateOrder(cart);
                }

                _logger.Error(ex.Message, ex);

                throw;
            }
        }

        private ICart UpdateCartWithOrderData(ICart cart, CheckoutOrderData orderData)
        {
            var shipment = cart.GetFirstShipment();
            if (shipment != null && orderData.ShippingAddress != null)
            {
                shipment.ShippingAddress = orderData.ShippingAddress.ToOrderAddress(cart);
                _orderRepository.Save(cart);
            }

            // Store checkout order id on cart
            cart.Properties[Constants.KlarnaCheckoutOrderIdCartField] = orderData.OrderId;

            _orderRepository.Save(cart);

            return cart;
        }

        private CheckoutOrderData GetCheckoutOrderData(ICart cart, IMarket market, PaymentMethodDto paymentMethodDto)
        {
            var totals = _orderGroupCalculator.GetOrderGroupTotals(cart);
            var shipment = cart.GetFirstShipment();
            var marketCountry = CountryCodeHelper.GetTwoLetterCountryCode(market.Countries.FirstOrDefault());
            if (string.IsNullOrWhiteSpace(marketCountry))
            {
                throw new ConfigurationException($"Please select a country in CM for market {cart.MarketId}");
            }
            var checkoutConfiguration = GetCheckoutConfiguration(market);

            var orderData = new PatchedCheckoutOrderData
            {
                PurchaseCountry = marketCountry,
                PurchaseCurrency = cart.Currency.CurrencyCode,
                Locale = ContentLanguage.PreferredCulture.Name,
                // Non-negative, minor units. Total amount of the order, including tax and any discounts.
                OrderAmount = AmountHelper.GetAmount(totals.Total),
                // Non-negative, minor units. The total tax amount of the order.
                OrderTaxAmount = AmountHelper.GetAmount(totals.TaxTotal),
                MerchantUrls = GetMerchantUrls(cart),
                OrderLines = GetOrderLines(cart, totals, checkoutConfiguration.SendProductAndImageUrl)
            };

            if (checkoutConfiguration.SendShippingCountries)
            {
                orderData.ShippingCountries = GetCountries().ToList();
            }

            // KCO_6 Setting to let the user select shipping options in the iframe
            if (checkoutConfiguration.SendShippingOptionsPriorAddresses)
            {
                if (checkoutConfiguration.ShippingOptionsInIFrame)
                {
                    orderData.ShippingOptions = GetShippingOptions(cart, cart.Currency, ContentLanguage.PreferredCulture);
                }
                else
                {
                    if (shipment != null)
                    {
                        orderData.SelectedShippingOption = ShippingManager.GetShippingMethod(shipment.ShippingMethodId)
                            ?.ShippingMethod?.FirstOrDefault()
                            ?.ToShippingOption();
                    }
                }
            }

            if (paymentMethodDto != null)
            {
                orderData.Options = GetOptions(cart.MarketId);
            }
            return orderData;
        }

        private CheckoutOptions GetOptions(MarketId marketId)
        {
            var configuration = GetConfiguration(marketId);
            var options = new PatchedCheckoutOptions
            {
                RequireValidateCallbackSuccess = configuration.RequireValidateCallbackSuccess,
                AllowSeparateShippingAddress = configuration.AllowSeparateShippingAddress,
                ColorButton = GetColor(configuration.WidgetButtonColor),
                ColorButtonText = GetColor(configuration.WidgetButtonTextColor),
                ColorCheckbox = GetColor(configuration.WidgetCheckboxColor),
                ColorCheckboxCheckmark = GetColor(configuration.WidgetCheckboxCheckmarkColor),
                ColorHeader = GetColor(configuration.WidgetHeaderColor),
                ColorLink = GetColor(configuration.WidgetLinkColor),
                RadiusBorder = configuration.WidgetBorderRadius,
                DateOfBirthMandatory = configuration.DateOfBirthMandatory,
                ShowSubtotalDetail = configuration.ShowSubtotalDetail,
                TitleMandatory = configuration.TitleMandatory,
                ShippingDetails = configuration.ShippingDetailsText
            };

            var additionalCheckboxText = configuration.AdditionalCheckboxText;
            if (!string.IsNullOrEmpty(additionalCheckboxText))
            {
                options.AdditionalCheckbox = new AdditionalCheckbox
                {
                    Text = additionalCheckboxText,
                    Checked = configuration.AdditionalCheckboxDefaultChecked,
                    Required = configuration.AdditionalCheckboxRequired
                };
            }
            return options;
        }

        private string GetColor(string colorString)
        {
            return string.IsNullOrWhiteSpace(colorString) ? null : colorString;
        }

        public CheckoutOrderData GetOrder(string klarnaOrderId, IMarket market)
        {
            var checkout = CreateCheckoutOrder(klarnaOrderId, market);
            return checkout.Fetch();
        }

        public ICart GetCartByKlarnaOrderId(int orderGroupdId, string orderId)
        {
            var cart = _orderRepository.Load<ICart>(orderGroupdId);
            return cart;
        }

        public ShippingOptionUpdateResponse UpdateShippingMethod(ICart cart, ShippingOptionUpdateRequest shippingOptionUpdateRequest)
        {
            var configuration = GetConfiguration(cart.MarketId);
            var shipment = cart.GetFirstShipment();
            if (shipment != null && Guid.TryParse(shippingOptionUpdateRequest.SelectedShippingOption.Id, out Guid guid))
            {
                shipment.ShippingMethodId = guid;
                shipment.ShippingAddress = shippingOptionUpdateRequest.ShippingAddress.ToOrderAddress(cart);
                _orderRepository.Save(cart);
            }

            var totals = _orderGroupCalculator.GetOrderGroupTotals(cart);
            var result =  new ShippingOptionUpdateResponse
            {
                OrderAmount = AmountHelper.GetAmount(totals.Total),
                OrderTaxAmount = AmountHelper.GetAmount(totals.TaxTotal),
                OrderLines = GetOrderLines(cart, totals, configuration.SendProductAndImageUrl),
                PurchaseCurrency = cart.Currency.CurrencyCode
            };

            if (ServiceLocator.Current.TryGetExistingInstance(out ICheckoutOrderDataBuilder checkoutOrderDataBuilder))
            {
                checkoutOrderDataBuilder.Build(result, cart, configuration);
            }
            return result;
        }

        public AddressUpdateResponse UpdateAddress(ICart cart, AddressUpdateRequest addressUpdateRequest)
        {
            var configuration = GetConfiguration(cart.MarketId);
            var shipment = cart.GetFirstShipment();
            if (shipment != null)
            {
                shipment.ShippingAddress = addressUpdateRequest.ShippingAddress.ToOrderAddress(cart);
                _orderRepository.Save(cart);
            }

            var totals = _orderGroupCalculator.GetOrderGroupTotals(cart);
            var result = new AddressUpdateResponse
            {
                OrderAmount = AmountHelper.GetAmount(totals.Total),
                OrderTaxAmount = AmountHelper.GetAmount(totals.TaxTotal),
                OrderLines = GetOrderLines(cart, totals, configuration.SendProductAndImageUrl),
                PurchaseCurrency = cart.Currency.CurrencyCode,
                ShippingOptions = configuration.ShippingOptionsInIFrame ? GetShippingOptions(cart, cart.Currency, ContentLanguage.PreferredCulture) : Enumerable.Empty<ShippingOption>()
            };

            if (ServiceLocator.Current.TryGetExistingInstance(out ICheckoutOrderDataBuilder checkoutOrderDataBuilder))
            {
                checkoutOrderDataBuilder.Build(result, cart, configuration);
            }
            return result;
        }

        public bool ValidateOrder(ICart cart, PatchedCheckoutOrderData checkoutData)
        {
            // Compare the current cart state to the Klarna order state (totals, shipping selection, discounts, and gift cards). If they don't match there is an issue.
            var market = _marketService.GetMarket(cart.MarketId);
            var localCheckoutOrderData = (PatchedCheckoutOrderData)GetCheckoutOrderData(cart, market, PaymentMethodDto);
            localCheckoutOrderData.ShippingAddress = cart.GetFirstShipment().ShippingAddress.ToAddress();
            if (!_klarnaOrderValidator.Compare(checkoutData, localCheckoutOrderData))
            {
                return false;
            }

            // Order is valid, create on hold cart in epi
            cart.Name = OrderStatus.OnHold.ToString();
            _orderRepository.Save(cart);

            // Create new default cart
            var newCart = _orderRepository.Create<ICart>(cart.CustomerId, "Default");
            _orderRepository.Save(newCart);

            return true;
        }

        public void CancelOrder(ICart cart)
        {
            var klarnaOrderService = _klarnaOrderServiceFactory.Create(GetConfiguration(cart.MarketId));

            var orderId = cart.Properties[Constants.KlarnaCheckoutOrderIdCartField]?.ToString();
            if (!string.IsNullOrWhiteSpace(orderId))
            {
                klarnaOrderService.CancelOrder(orderId);

                cart.Properties[Constants.KlarnaCheckoutOrderIdCartField] = null;
                _orderRepository.Save(cart);
            }
        }

        public void UpdateMerchantReference1(IPurchaseOrder purchaseOrder)
        {
            var klarnaOrderService = _klarnaOrderServiceFactory.Create(GetConfiguration(purchaseOrder.MarketId));

            var orderId = purchaseOrder.Properties[Common.Constants.KlarnaOrderIdField]?.ToString();
            if (!string.IsNullOrWhiteSpace(orderId) && purchaseOrder is PurchaseOrder)
            {
                klarnaOrderService.UpdateMerchantReferences(orderId, ((PurchaseOrder)purchaseOrder).TrackingNumber, null);
            }
        }

        public void AcknowledgeOrder(IPurchaseOrder purchaseOrder)
        {
            var klarnaOrderService = _klarnaOrderServiceFactory.Create(GetConfiguration(purchaseOrder.MarketId));
            klarnaOrderService.AcknowledgeOrder(purchaseOrder);
        }

        public CheckoutConfiguration GetConfiguration(IMarket market)
        {
            return GetConfiguration(market.MarketId);
        }

        public CheckoutConfiguration GetConfiguration(MarketId marketId)
        {
            return _checkoutConfigurationLoader.GetConfiguration(marketId);
        }

        public void Complete(IPurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null)
            {
                throw new ArgumentNullException(nameof(purchaseOrder));
            }
            var orderForm = purchaseOrder.GetFirstForm();
            var payment = orderForm?.Payments.FirstOrDefault(x => x.PaymentMethodName.Equals(Constants.KlarnaCheckoutSystemKeyword));
            if (payment == null)
            {
                return;
            }

            var fraudStatus = payment.Properties[Common.Constants.FraudStatusPaymentField]?.ToString();
            if (fraudStatus == FraudStatus.PENDING.ToString())
            {
                OrderStatusManager.HoldOrder((PurchaseOrder)purchaseOrder);
                _orderRepository.Save(purchaseOrder);
            }
        }

        private IEnumerable<ShippingOption> GetShippingOptions(ICart cart, Currency currency, CultureInfo preferredCulture)
        {
            var methods = ShippingManager.GetShippingMethodsByMarket(cart.MarketId.Value, false);
            var currentLanguage = preferredCulture.TwoLetterISOLanguageName;

            var shippingOptions = methods.ShippingMethod
                .Where(shippingMethodRow => currentLanguage.Equals(shippingMethodRow.LanguageId,
                                                StringComparison.OrdinalIgnoreCase)
                                            &&
                                            string.Equals(currency, shippingMethodRow.Currency,
                                                StringComparison.OrdinalIgnoreCase))
                .Select(method => method.ToShippingOption())
                .ToList();
            var shipment = cart.GetFirstShipment();
            if (shipment == null)
            {
                return shippingOptions;
            }

            // Try to preselect a shipping option
            var selectedShipment =
                shippingOptions.FirstOrDefault(x => x.Id.Equals(shipment.ShippingMethodId.ToString()));
            if (selectedShipment != null)
            {
                selectedShipment.PreSelected = true;
            }
            return shippingOptions;
        }

        private PatchedMerchantUrls GetMerchantUrls(ICart cart)
        {
            if (PaymentMethodDto == null) return null;

            var configuration = GetConfiguration(cart.MarketId);

            Uri ToUri(Func<CheckoutConfiguration, string> fieldSelector)
            {
                var url = fieldSelector(configuration).Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString());
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    return uri;
                }

                if (Uri.TryCreate(configuration.BaseUrl, UriKind.Absolute, out var baseUri))
                {
                    return new Uri(baseUri, url);
                }

                return null;
            }

            return new PatchedMerchantUrls
            {
                Terms = ToUri(c => c.TermsUrl),
                CancellationTerms = !string.IsNullOrEmpty(configuration.CancellationTermsUrl) ? ToUri(c => c.CancellationTermsUrl) : null,
                Checkout = ToUri(c => c.CheckoutUrl),
                Confirmation = ToUri(c => c.ConfirmationUrl),
                Push = ToUri(c => c.PushUrl),
                AddressUpdate = ToUri(c => c.AddressUpdateUrl),
                ShippingOptionUpdate = ToUri(c => c.ShippingOptionUpdateUrl),
                Notification = ToUri(c => c.NotificationUrl),
                Validation = ToUri(c => c.OrderValidationUrl)
            };
        }

        private IEnumerable<string> GetCountries()
        {
            var countries = CountryManager.GetCountries();
            return CountryCodeHelper.GetTwoLetterCountryCodes(countries.Country.Select(x => x.Code));
        }

        private ICheckoutOrder CreateCheckoutOrder(IMarket market)
        {
            return CreateCheckoutOrder(market, client => client.NewCheckoutOrder());
        }

        private ICheckoutOrder CreateCheckoutOrder(string klarnaOrderId, IMarket market)
        {
            return CreateCheckoutOrder(market, client => client.NewCheckoutOrder(klarnaOrderId));
        }

        private ICheckoutOrder CreateCheckoutOrder(IMarket market, Func<Client, ICheckoutOrder> factory)
        {
            var client = GetClient(market);
            var checkoutOrder = factory(client);
            return new LoggingCheckoutOrder(checkoutOrder);
        }
    }
}