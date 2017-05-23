using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using EPiServer.Business.Commerce.Exception;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using EPiServer.Logging;
using Klarna.Checkout.Extensions;
using Klarna.Checkout.Models;
using Klarna.Common;
using Klarna.Common.Extensions;
using Klarna.Common.Helpers;
using Klarna.OrderManagement;
using Klarna.Rest;
using Klarna.Rest.Models;
using Klarna.Rest.Transport;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Newtonsoft.Json;
using ConfigurationException = EPiServer.Business.Commerce.Exception.ConfigurationException;

namespace Klarna.Checkout
{
    [ServiceConfiguration(typeof(IKlarnaCheckoutService))]
    public class KlarnaCheckoutService : KlarnaService, IKlarnaCheckoutService
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(KlarnaCheckoutService));
        private readonly IOrderGroupTotalsCalculator _orderGroupTotalsCalculator;

        private readonly IKlarnaOrderValidator _klarnaOrderValidator;
        private readonly IOrderRepository _orderRepository;
        private readonly IKlarnaOrderService _klarnaOrderService;
        private readonly ICurrentMarket _currentMarket;

        private Client _client;
        private PaymentMethodDto _paymentMethodDto;
        private CheckoutConfiguration _checkoutConfiguration;

        public KlarnaCheckoutService(
            IOrderGroupTotalsCalculator orderGroupTotalsCalculator,
            IOrderRepository orderRepository,
            IPaymentProcessor paymentProcessor,
            IOrderGroupCalculator orderGroupCalculator,
            IKlarnaOrderValidator klarnaOrderValidator, 
            ICurrentMarket currentMarket) : base(orderRepository, paymentProcessor, orderGroupCalculator)
        {
            _orderGroupTotalsCalculator = orderGroupTotalsCalculator;
            _orderRepository = orderRepository;
            _klarnaOrderValidator = klarnaOrderValidator;
            _currentMarket = currentMarket;
            _klarnaOrderService = new KlarnaOrderService(CheckoutConfiguration);
        }

        public PaymentMethodDto PaymentMethodDto
        {
            get
            {
                if (_paymentMethodDto == null)
                {
                    _paymentMethodDto = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaCheckoutSystemKeyword, ContentLanguage.PreferredCulture.Name);
                }
                return _paymentMethodDto;
            }
        }

        public CheckoutConfiguration CheckoutConfiguration
        {
            get
            {
                if (_checkoutConfiguration == null)
                {
                    _checkoutConfiguration = GetConfiguration(_currentMarket.GetCurrentMarket());
                }
                return _checkoutConfiguration;
            }
        }

        public Client Client
        {
            get
            {
                if (_client == null)
                {
                    if (PaymentMethodDto != null)
                    {
                        var connectionConfiguration = CheckoutConfiguration;
                        var connector = ConnectorFactory.Create(connectionConfiguration.Username,
                            connectionConfiguration.Password, new Uri(connectionConfiguration.ApiUrl));

                        _client = new Client(connector);
                    }
                }
                return _client;
            }
        }

        public CheckoutOrderData CreateOrUpdateOrder(ICart cart)
        {
            var orderId = cart.Properties[Constants.KlarnaCheckoutOrderIdField]?.ToString();
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
            var checkout = Client.NewCheckoutOrder();
            var orderData = GetCheckoutOrderData(cart, PaymentMethodDto);

            if (CheckoutConfiguration.PrefillAddress)
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
                    checkoutOrderDataBuilder.Build(orderData, cart, CheckoutConfiguration);
                }
                checkout.Create(orderData);
                orderData = checkout.Fetch();

                cart = UpdateCartWithOrderData(cart, orderData);

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
            var checkout = Client.NewCheckoutOrder(orderId);
            var orderData = GetCheckoutOrderData(cart, PaymentMethodDto);

            try
            {
                if (ServiceLocator.Current.TryGetExistingInstance(out ICheckoutOrderDataBuilder checkoutOrderDataBuilder))
                {
                    checkoutOrderDataBuilder.Build(orderData, cart, CheckoutConfiguration);
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
            cart.Properties[Constants.KlarnaCheckoutOrderIdField] = orderData.OrderId;

            _orderRepository.Save(cart);

            return cart;
        }

        private CheckoutOrderData GetCheckoutOrderData(ICart cart, PaymentMethodDto paymentMethodDto)
        {
            var totals = _orderGroupTotalsCalculator.GetTotals(cart);
            var shipment = cart.GetFirstShipment();

            var orderData = new PatchedCheckoutOrderData
            {
                PurchaseCountry = CountryCodeHelper.GetTwoLetterCountryCode(cart.Market.Countries.FirstOrDefault()),
                PurchaseCurrency = cart.Currency.CurrencyCode,
                Locale = ContentLanguage.PreferredCulture.Name,
                // Non-negative, minor units. Total amount of the order, including tax and any discounts.
                OrderAmount = AmountHelper.GetAmount(totals.Total),
                // Non-negative, minor units. The total tax amount of the order.
                OrderTaxAmount = AmountHelper.GetAmount(totals.TaxTotal),
                MerchantUrls = GetMerchantUrls(cart),
                OrderLines = GetOrderLines(cart, totals, false)
            };

            if (CheckoutConfiguration.SendShippingCountries)
            {
                orderData.ShippingCountries = GetCountries().ToList();
            }

            // KCO_6 Setting to let the user select shipping options in the iframe
            if (CheckoutConfiguration.SendShippingOptionsPriorAddresses)
            {
                if (CheckoutConfiguration.ShippingOptionsInIFrame)
                {
                    orderData.ShippingOptions = GetShippingOptions(cart);
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
                orderData.Options = GetOptions(cart);
            }
            return orderData;
        }

        private CheckoutOptions GetOptions(ICart cart)
        {
            var configuration = GetConfiguration(cart.Market);
            var options = new PatchedCheckoutOptions
            {
                RequireValidateCallbackSuccess = configuration.RequireValidateCallbackSuccess,
                AllowSeparateShippingAddress = configuration.AllowSeparateShippingAddress,
                ColorButton = configuration.WidgetButtonColor,
                ColorButtonText = configuration.WidgetButtonTextColor,
                ColorCheckbox = configuration.WidgetCheckboxColor,
                ColorCheckboxCheckmark = configuration.WidgetCheckboxCheckmarkColor,
                ColorHeader = configuration.WidgetHeaderColor,
                ColorLink = configuration.WidgetLinkColor,
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

        public CheckoutOrderData GetOrder(string orderId)
        {
            var checkout = Client.NewCheckoutOrder(orderId);

            return checkout.Fetch();
        }

        public ICart GetCartByKlarnaOrderId(int orderGroupdId, string orderId)
        {
            var cart = _orderRepository.Load<ICart>(orderGroupdId);
            return cart;
        }

        public ShippingOptionUpdateResponse UpdateShippingMethod(ICart cart, ShippingOptionUpdateRequest shippingOptionUpdateRequest)
        {
            var shipment = cart.GetFirstShipment();
            if (shipment != null && Guid.TryParse(shippingOptionUpdateRequest.SelectedShippingOption.Id, out Guid guid))
            {
                shipment.ShippingMethodId = guid;
                shipment.ShippingAddress = shippingOptionUpdateRequest.ShippingAddress.ToOrderAddress(cart);
                _orderRepository.Save(cart);
            }

            var totals = _orderGroupTotalsCalculator.GetTotals(cart);
            return new ShippingOptionUpdateResponse
            {
                OrderAmount = AmountHelper.GetAmount(totals.Total),
                OrderTaxAmount = AmountHelper.GetAmount(totals.TaxTotal),
                OrderLines = GetOrderLines(cart, totals, false),
                PurchaseCurrency = cart.Currency.CurrencyCode
            };
        }

        public AddressUpdateResponse UpdateAddress(ICart cart, AddressUpdateRequest addressUpdateRequest)
        {
            var shipment = cart.GetFirstShipment();
            if (shipment != null)
            {
                shipment.ShippingAddress = addressUpdateRequest.ShippingAddress.ToOrderAddress(cart);
                _orderRepository.Save(cart);
            }
            
            var totals = _orderGroupTotalsCalculator.GetTotals(cart);
            return new AddressUpdateResponse
            {
                OrderAmount = AmountHelper.GetAmount(totals.Total),
                OrderTaxAmount = AmountHelper.GetAmount(totals.TaxTotal),
                OrderLines = GetOrderLines(cart, totals, false),
                PurchaseCurrency = cart.Currency.CurrencyCode,
                ShippingOptions = CheckoutConfiguration.ShippingOptionsInIFrame ? GetShippingOptions(cart) : Enumerable.Empty<ShippingOption>()
            };
        }

        public bool ValidateOrder(ICart cart, PatchedCheckoutOrderData checkoutData)
        {
            // Compare the current cart state to the Klarna order state (totals, shipping selection, discounts, and gift cards). If they don't match there is an issue.
            var localCheckoutOrderData = GetCheckoutOrderData(cart, PaymentMethodDto) as PatchedCheckoutOrderData;
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
            var orderId = cart.Properties[Constants.KlarnaCheckoutOrderIdField]?.ToString();
            if (!string.IsNullOrWhiteSpace(orderId))
            {
                _klarnaOrderService.CancelOrder(orderId);

                cart.Properties[Constants.KlarnaCheckoutOrderIdField] = null;
                _orderRepository.Save(cart);
            }
        }

        public void UpdateMerchantReference1(IPurchaseOrder purchaseOrder)
        {
            var orderId = purchaseOrder.Properties[Constants.KlarnaCheckoutOrderIdField]?.ToString();
            if (!string.IsNullOrWhiteSpace(orderId) && purchaseOrder is PurchaseOrder)
            {
                _klarnaOrderService.UpdateMerchantReferences(orderId, ((PurchaseOrder)purchaseOrder).TrackingNumber, null);
            }
        }

        public void AcknowledgeOrder(IPurchaseOrder purchaseOrder)
        {
            _klarnaOrderService.AcknowledgeOrder(purchaseOrder);
        }

        public CheckoutConfiguration GetConfiguration(IMarket market)
        {
            return GetConfiguration(market.MarketId);
        }

        public CheckoutConfiguration GetConfiguration(MarketId marketId)
        {
            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaCheckoutSystemKeyword, ContentLanguage.PreferredCulture.Name);
            if (paymentMethod == null)
            {
                throw new Exception(
                    $"PaymentMethod {Constants.KlarnaCheckoutSystemKeyword} is not configured for market {marketId} and language {ContentLanguage.PreferredCulture.Name}");
            }
            return paymentMethod.GetKlarnaCheckoutConfiguration(marketId);
        }

        private IEnumerable<ShippingOption> GetShippingOptions(ICart cart)
        {
            var methods = ShippingManager.GetShippingMethodsByMarket(cart.Market.MarketId.Value, false);
            var shippingOptions = methods.ShippingMethod.Select(method => method.ToShippingOption()).ToList();
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

        private MerchantUrls GetMerchantUrls(ICart cart)
        {
            if (PaymentMethodDto != null)
            {
                var configuration = GetConfiguration(cart.Market);
                return new MerchantUrls
                {
                    Terms = new Uri(configuration.TermsUrl),
                    Checkout = new Uri(configuration.CheckoutUrl),
                    Confirmation = new Uri(configuration.ConfirmationUrl.Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString())),
                    Push = new Uri(configuration.PushUrl.Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString())),
                    AddressUpdate = new Uri(configuration.AddressUpdateUrl.Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString())),
                    ShippingOptionUpdate = new Uri(configuration.ShippingOptionUpdateUrl.Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString())),
                    Notification = new Uri(configuration.NotificationUrl.Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString())),
                    Validation = new Uri(configuration.OrderValidationUrl.Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString()))
                };
            }
            return null;
        }

        private IEnumerable<string> GetCountries()
        {
            var countries = CountryManager.GetCountries();
            return CountryCodeHelper.GetTwoLetterCountryCodes(countries.Country.Select(x => x.Code));
        }
    }
}