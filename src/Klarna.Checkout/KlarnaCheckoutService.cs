using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using EPiServer.Logging;
using Klarna.Checkout.Extensions;
using Klarna.Checkout.Models;
using Klarna.Common;
using Klarna.Common.Extensions;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Klarna.OrderManagement;
using Klarna.Payments.Models;
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
        private readonly ICheckoutLanguageIdConverter _checkoutLanguageIdConverter;
        private readonly IKlarnaCartValidator _klarnaCartValidator;

        private CheckoutStore _client;
        private PaymentMethodDto _paymentMethodDto;
        private CheckoutConfiguration _checkoutConfiguration;

        public KlarnaCheckoutService(
            IOrderRepository orderRepository,
            IPaymentProcessor paymentProcessor,
            IOrderGroupCalculator orderGroupCalculator,
            IKlarnaOrderValidator klarnaOrderValidator,
            IMarketService marketService,
            ICheckoutConfigurationLoader checkoutConfigurationLoader,
            KlarnaOrderServiceFactory klarnaOrderServiceFactory,
            ICheckoutLanguageIdConverter checkoutLanguageIdConverter,
            IKlarnaCartValidator klarnaCartValidator)
            : base(orderRepository, paymentProcessor, orderGroupCalculator, marketService)
        {
            _orderGroupCalculator = orderGroupCalculator;
            _orderRepository = orderRepository;
            _klarnaOrderValidator = klarnaOrderValidator;
            _marketService = marketService;
            _checkoutConfigurationLoader = checkoutConfigurationLoader;
            _klarnaOrderServiceFactory = klarnaOrderServiceFactory;
            _checkoutLanguageIdConverter = checkoutLanguageIdConverter;
            _klarnaCartValidator = klarnaCartValidator;
        }

        public virtual PaymentMethodDto PaymentMethodDto =>
            _paymentMethodDto
            ?? (_paymentMethodDto =
                PaymentManager.GetPaymentMethodBySystemName(
                    Constants.KlarnaCheckoutSystemKeyword, ContentLanguage.PreferredCulture.Name, returnInactive: true));

        public virtual CheckoutConfiguration GetCheckoutConfiguration(IMarket market)
        {
            return _checkoutConfiguration ?? (_checkoutConfiguration = GetConfiguration(market.MarketId));
        }

        public virtual CheckoutStore GetClient(IMarket market)
        {
            if (PaymentMethodDto != null)
            {
                var connectionConfiguration = GetCheckoutConfiguration(market);

                string userAgent = $"Platform/Episerver.Commerce_{typeof(EPiServer.Commerce.ApplicationContext).Assembly.GetName().Version} Module/Klarna.Checkout_{typeof(KlarnaCheckoutService).Assembly.GetName().Version}";
                
                _client =  new CheckoutStore(new ApiSession
                {
                    ApiUrl = connectionConfiguration.ApiUrl,
                    UserAgent = userAgent,
                    Credentials = new ApiCredentials
                    {
                        Username = connectionConfiguration.Username,
                        Password = connectionConfiguration.Password
                    }
                }, new JsonSerializer());
            }
            return _client;
        }

        public virtual async Task<CheckoutOrder> CreateOrUpdateOrder(ICart cart)
        {
            var orderId = cart.Properties[Constants.KlarnaCheckoutOrderIdCartField]?.ToString();
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return await CreateOrder(cart);
            }
            else
            {
                return await UpdateOrder(orderId, cart).ConfigureAwait(false);
            }
        }

        public virtual async Task<CheckoutOrder> CreateOrder(ICart cart)
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
                    orderData.BillingCheckoutAddress = customerContact.PreferredBillingAddress.ToAddress();
                }

                if (orderData.CheckoutOptions.AllowSeparateShippingAddress)
                {
                    var shipment = cart.GetFirstShipment();
                    if (shipment.ShippingAddress != null)
                    {
                        orderData.ShippingCheckoutAddress = shipment.ShippingAddress.ToCheckoutAddress();
                    }
                }
            }
            try
            {
                if (ServiceLocator.Current.TryGetExistingInstance(out ICheckoutOrderDataBuilder checkoutOrderDataBuilder))
                {
                    checkoutOrderDataBuilder.Build(orderData, cart, checkoutConfiguration);
                }

                orderData = await checkout.Create(orderData).ConfigureAwait(false);

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

        public virtual async Task<CheckoutOrder> UpdateOrder(string orderId, ICart cart)
        {
            var market = _marketService.GetMarket(cart.MarketId);
            var checkout = CreateCheckoutOrder(market);
            var orderData = GetCheckoutOrderData(cart, market, PaymentMethodDto);
            var checkoutConfiguration = GetCheckoutConfiguration(market);

            try
            {
                if (ServiceLocator.Current.TryGetExistingInstance(out ICheckoutOrderDataBuilder checkoutOrderDataBuilder))
                {
                    checkoutOrderDataBuilder.Build(orderData, cart, checkoutConfiguration);
                }

                orderData = await checkout.Update(orderData).ConfigureAwait(false);

                cart = UpdateCartWithOrderData(cart, orderData);

                return orderData;
            }
            catch (ApiException ex)
            {
                // Create new session if current one is not found
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return await CreateOrder(cart).ConfigureAwait(false);
                }

                _logger.Error($"{ex.ErrorMessage.CorrelationId} {ex.ErrorMessage.ErrorCode} {string.Join(", ", ex.ErrorMessage.ErrorMessages)}", ex);

                throw;
            }
            catch (WebException ex)
            {
                if ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                {
                    return await CreateOrder(cart).ConfigureAwait(false);
                }

                _logger.Error(ex.Message, ex);

                throw;
            }
        }

        protected virtual ICart UpdateCartWithOrderData(ICart cart, CheckoutOrder orderData)
        {
            var shipment = cart.GetFirstShipment();
            if (shipment != null && orderData.ShippingCheckoutAddress.IsValid())
            {
                shipment.ShippingAddress = orderData.ShippingCheckoutAddress.ToOrderAddress(cart);
                _orderRepository.Save(cart);
            }

            // Store checkout order id on cart
            cart.Properties[Constants.KlarnaCheckoutOrderIdCartField] = orderData.OrderId;

            _orderRepository.Save(cart);

            return cart;
        }

        protected virtual CheckoutOrder GetCheckoutOrderData(
            ICart cart, IMarket market, PaymentMethodDto paymentMethodDto)
        {
            var totals = _orderGroupCalculator.GetOrderGroupTotals(cart);
            var shipment = cart.GetFirstShipment();
            var marketCountry = CountryCodeHelper.GetTwoLetterCountryCode(market.Countries.FirstOrDefault());
            if (string.IsNullOrWhiteSpace(marketCountry))
            {
                throw new ConfigurationException($"Please select a country in CM for market {cart.MarketId}");
            }
            var checkoutConfiguration = GetCheckoutConfiguration(market);

            var orderData = new CheckoutOrder
            {
                PurchaseCountry = marketCountry,
                PurchaseCurrency = cart.Currency.CurrencyCode,
                Locale = $"{_checkoutLanguageIdConverter.ConvertToCheckoutLanguageId(ContentLanguage.PreferredCulture.Name)}-{marketCountry}",
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
                    orderData.ShippingOptions = GetShippingOptions(cart, cart.Currency, ContentLanguage.PreferredCulture).ToList();
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
                orderData.CheckoutOptions = GetOptions(cart.MarketId);
            }
            return orderData;
        }

        protected virtual CheckoutOptions GetOptions(MarketId marketId)
        {
            var configuration = GetConfiguration(marketId);
            var options = new CheckoutOptions
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

        protected virtual string GetColor(string colorString)
        {
            return string.IsNullOrWhiteSpace(colorString) ? null : colorString;
        }

        public virtual async Task<CheckoutOrder> GetOrder(string klarnaOrderId, IMarket market)
        {
            var checkout = CreateCheckoutOrder(market);
            return await checkout.Fetch(klarnaOrderId).ConfigureAwait(false);
        }

        public virtual ICart GetCartByKlarnaOrderId(int orderGroupdId, string orderId)
        {
            var cart = _orderRepository.Load<ICart>(orderGroupdId);
            return cart;
        }

        public virtual ShippingOptionUpdateResponse UpdateShippingMethod(ICart cart, ShippingOptionUpdateRequest shippingOptionUpdateRequest)
        {
            var configuration = GetConfiguration(cart.MarketId);
            var shipment = cart.GetFirstShipment();
            var validationIssues = new Dictionary<ILineItem, List<ValidationIssue>>();
            var rewardDescriptions = Enumerable.Empty<RewardDescription>();
            
            if (shipment != null && Guid.TryParse(shippingOptionUpdateRequest.SelectedShippingOption.Id, out Guid guid))
            {
                shipment.ShippingAddress = shippingOptionUpdateRequest.ShippingCheckoutAddress.ToOrderAddress(cart);
                shipment.ShippingMethodId = guid;
                validationIssues = _klarnaCartValidator.ValidateCart(cart);
                rewardDescriptions = _klarnaCartValidator.ApplyDiscounts(cart);
                _orderRepository.Save(cart);
            }

            var totals = _orderGroupCalculator.GetOrderGroupTotals(cart);
            var result =  new ShippingOptionUpdateResponse
            {
                OrderAmount = AmountHelper.GetAmount(totals.Total),
                OrderTaxAmount = AmountHelper.GetAmount(totals.TaxTotal),
                OrderLines = GetOrderLines(cart, totals, configuration.SendProductAndImageUrl),
                PurchaseCurrency = cart.Currency.CurrencyCode,
                ShippingOptions = configuration.ShippingOptionsInIFrame 
                    ? GetShippingOptions(cart, cart.Currency, ContentLanguage.PreferredCulture)
                    : Enumerable.Empty<ShippingOption>(),
                ValidationIssues = validationIssues,
                RewardDescriptions = rewardDescriptions
            };

            if (ServiceLocator.Current.TryGetExistingInstance(out ICheckoutOrderDataBuilder checkoutOrderDataBuilder))
            {
                checkoutOrderDataBuilder.Build(result, cart, configuration);
            }
            
            return result;
        }

        public virtual AddressUpdateResponse UpdateAddress(ICart cart, CallbackAddressUpdateRequest addressUpdateRequest)
        {
            var configuration = GetConfiguration(cart.MarketId);
            var shipment = cart.GetFirstShipment();
            var validationIssues = new Dictionary<ILineItem, List<ValidationIssue>>();
            var rewardDescriptions = Enumerable.Empty<RewardDescription>();
            
            if (shipment != null)
            {
                shipment.ShippingAddress = addressUpdateRequest.ShippingCheckoutAddress.ToOrderAddress(cart);
                validationIssues = _klarnaCartValidator.ValidateCart(cart);
                rewardDescriptions = _klarnaCartValidator.ApplyDiscounts(cart);
                _orderRepository.Save(cart);
            }

            var totals = _orderGroupCalculator.GetOrderGroupTotals(cart);
            var result = new AddressUpdateResponse
            {
                OrderAmount = AmountHelper.GetAmount(totals.Total),
                OrderTaxAmount = AmountHelper.GetAmount(totals.TaxTotal),
                OrderLines = GetOrderLines(cart, totals, configuration.SendProductAndImageUrl),
                PurchaseCurrency = cart.Currency.CurrencyCode,
                ShippingOptions = configuration.ShippingOptionsInIFrame
                    ? GetShippingOptions(cart, cart.Currency, ContentLanguage.PreferredCulture)
                    : Enumerable.Empty<ShippingOption>(),
                ValidationIssues = validationIssues,
                RewardDescriptions = rewardDescriptions
            };

            if (ServiceLocator.Current.TryGetExistingInstance(out ICheckoutOrderDataBuilder checkoutOrderDataBuilder))
            {
                checkoutOrderDataBuilder.Build(result, cart, configuration);
            }
            
            return result;
        }

        public virtual bool ValidateOrder(ICart cart, CheckoutOrder checkoutData)
        {
            // Compare the current cart state to the Klarna order state (totals, shipping selection, discounts, and gift cards). If they don't match there is an issue.
            var market = _marketService.GetMarket(cart.MarketId);
            var localCheckoutOrderData = GetCheckoutOrderData(cart, market, PaymentMethodDto);
            localCheckoutOrderData.ShippingCheckoutAddress = cart.GetFirstShipment().ShippingAddress.ToCheckoutAddress();
            
            if (!_klarnaOrderValidator.Compare(checkoutData, localCheckoutOrderData))
            {
                return false;
            }

            // Order is valid, create on hold cart in epi
            cart.Name = OrderStatus.OnHold.ToString();
            _orderRepository.Save(cart);

            // Create new default cart
            var newCart = _orderRepository.Create<ICart>(cart.CustomerId, Cart.DefaultName);
            _orderRepository.Save(newCart);

            return true;
        }

        public virtual void CancelOrder(ICart cart)
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

        public virtual async Task UpdateMerchantReference1(IPurchaseOrder purchaseOrder)
        {
            var klarnaOrderService = _klarnaOrderServiceFactory.Create(GetConfiguration(purchaseOrder.MarketId));

            var orderId = purchaseOrder.Properties[Common.Constants.KlarnaOrderIdField]?.ToString();
            if (!string.IsNullOrWhiteSpace(orderId) && purchaseOrder is PurchaseOrder)
            {
                await klarnaOrderService.UpdateMerchantReferences(orderId, ((PurchaseOrder)purchaseOrder).TrackingNumber, null).ConfigureAwait(false);
            }
        }

        public virtual void AcknowledgeOrder(IPurchaseOrder purchaseOrder)
        {
            var klarnaOrderService = _klarnaOrderServiceFactory.Create(GetConfiguration(purchaseOrder.MarketId));
            klarnaOrderService.AcknowledgeOrder(purchaseOrder);
        }

        public virtual CheckoutConfiguration GetConfiguration(IMarket market)
        {
            return GetConfiguration(market.MarketId);
        }

        public virtual CheckoutConfiguration GetConfiguration(MarketId marketId)
        {
            return _checkoutConfigurationLoader.GetConfiguration(marketId);
        }

        public virtual void Complete(IPurchaseOrder purchaseOrder)
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

            if (payment.HasFraudStatus(FraudStatus.PENDING))
            {
                OrderStatusManager.HoldOrder((PurchaseOrder)purchaseOrder);
                _orderRepository.Save(purchaseOrder);
            }
        }

        protected virtual IEnumerable<ShippingOption> GetShippingOptions(ICart cart, Currency currency, CultureInfo preferredCulture)
        {
            var methods = ShippingManager.GetShippingMethodsByMarket(cart.MarketId.Value, false);
            var currentLanguage = preferredCulture.Name;

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
                shippingOptions.ForEach(option => option.Preselected = false);
                selectedShipment.Preselected = true;
            }
            return shippingOptions;
        }

        protected virtual CheckoutMerchantUrls GetMerchantUrls(ICart cart)
        {
            if (PaymentMethodDto == null) return null;

            var configuration = GetConfiguration(cart.MarketId);

            Uri ToFullSiteUrl(Func<CheckoutConfiguration, string> fieldSelector)
            {
                var url = fieldSelector(configuration).Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString());
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    return uri;
                }

                return new Uri(SiteUrlHelper.GetCurrentSiteUrl(), url);
            }

            return new CheckoutMerchantUrls
            {
                Terms = ToFullSiteUrl(c => c.TermsUrl).ToString(),
                CancellationTerms = !string.IsNullOrEmpty(configuration.CancellationTermsUrl) ? ToFullSiteUrl(c => c.CancellationTermsUrl).ToString() : null,
                Checkout = ToFullSiteUrl(c => c.CheckoutUrl).ToString(),
                Confirmation = ToFullSiteUrl(c => c.ConfirmationUrl).ToString(),
                Push = ToFullSiteUrl(c => c.PushUrl).ToString(),
                AddressUpdate = ToFullSiteUrl(c => c.AddressUpdateUrl).ToString(),
                ShippingOptionUpdate = ToFullSiteUrl(c => c.ShippingOptionUpdateUrl).ToString(),
                Notification = ToFullSiteUrl(c => c.NotificationUrl).ToString(),
                Validation = ToFullSiteUrl(c => c.OrderValidationUrl).ToString()
            };
        }

        protected virtual IEnumerable<string> GetCountries()
        {
            var countries = CountryManager.GetCountries();
            return CountryCodeHelper.GetTwoLetterCountryCodes(countries.Country.Select(x => x.Code));
        }

        protected virtual ICheckoutOrder CreateCheckoutOrder(IMarket market)
        {
            var client = GetClient(market);
            return new LoggingCheckoutOrder(client);
        }
    }
}