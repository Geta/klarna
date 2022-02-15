using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Klarna.Payments.Models;
using EPiServer.Logging;
using Klarna.Common;
using Klarna.Common.Configuration;
using Klarna.Common.Extensions;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Klarna.Payments.Extensions;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders.Managers;
using Options = Klarna.Payments.Models.Options;

namespace Klarna.Payments
{
    [ServiceConfiguration(typeof(IKlarnaPaymentsService))]
    public class KlarnaPaymentsService : KlarnaService, IKlarnaPaymentsService
    {
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IMarketService _marketService;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(KlarnaPaymentsService));
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly ILanguageService _languageService;
        private readonly IPurchaseOrderProcessor _purchaseOrderProcessor;
        private readonly IConfigurationLoader _configurationLoader;

        public KlarnaPaymentsService(
            IOrderRepository orderRepository,
            IOrderNumberGenerator orderNumberGenerator,
            IPaymentProcessor paymentProcessor,
            IOrderGroupCalculator orderGroupCalculator,
            IMarketService marketService,
            ILanguageService languageService,
            IPurchaseOrderProcessor purchaseOrderProcessor,
            IConfigurationLoader configurationLoader)
            : base(orderRepository, paymentProcessor, orderGroupCalculator, marketService, configurationLoader)
        {
            _orderGroupCalculator = orderGroupCalculator;
            _marketService = marketService;
            _orderRepository = orderRepository;
            _orderNumberGenerator = orderNumberGenerator;
            _languageService = languageService;
            _purchaseOrderProcessor = purchaseOrderProcessor;
            _configurationLoader = configurationLoader;
        }

        public async Task<bool> CreateOrUpdateSession(ICart cart, SessionSettings settings)
        {
            var sessionRequest = CreateSessionRequest(cart, settings);
            var sessionId = cart.GetKlarnaSessionId();

            if (string.IsNullOrEmpty(sessionId))
            {
                return await CreateSession(sessionRequest, cart, settings).ConfigureAwait(false);
            }

            try
            {
                await GetClient(cart.MarketId)
                    .UpdateSession(sessionId, sessionRequest)
                    .ConfigureAwait(false);

                return true;
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode == HttpStatusCode.NotFound)
                {
                    return await CreateSession(sessionRequest, cart, settings).ConfigureAwait(false);
                }

                _logger.Error(apiException.Message, apiException);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
        }

        private Session CreateSessionRequest(ICart cart, SessionSettings settings)
        {
            // Check if we shared PI before, if so it allows us to share it again
            var canSendPersonalInformation = AllowedToSharePersonalInformation(cart);
            var config = _configurationLoader.GetPaymentsConfiguration(cart.MarketId);

            var sessionRequest = GetSessionRequest(cart, config, settings.SiteUrl, canSendPersonalInformation);
            if (ServiceLocator.Current.TryGetExistingInstance(out ISessionBuilder sessionBuilder))
            {
                sessionRequest = sessionBuilder.Build(sessionRequest, cart, config, settings.AdditionalValues);
            }

            var market = _marketService.GetMarket(cart.MarketId);
            var currentCountry = market.Countries.FirstOrDefault();
            // Clear PI if we're not allowed to send it yet (can be set by custom session builder)
            if (!canSendPersonalInformation
                && !CanSendPersonalInformation(currentCountry))
            {
                // Can't share PI yet, will be done during the first authorize call
                sessionRequest.ShippingAddress = null;
                sessionRequest.BillingAddress = null;

                // If the pre assessment is not enabled then don't send the customer information to Klarna
                if (!config.CustomerPreAssessment)
                {
                    sessionRequest.Customer = null;
                }
            }

            return sessionRequest;
        }

        public async Task<Session> GetSession(ICart cart)
        {
            return await GetClient(cart.MarketId)
                .GetSession(cart.GetKlarnaSessionId())
                .ConfigureAwait(false);
        }

        public async Task<CreateOrderResponse> CreateOrder(string authorizationToken, ICart cart)
        {
            var config = _configurationLoader.GetPaymentsConfiguration(cart.MarketId);
            var sessionRequest = GetSessionRequest(cart, config, cart.GetSiteUrl(), true);

            sessionRequest.MerchantReference1 = _orderNumberGenerator.GenerateOrderNumber(cart);
            _orderRepository.Save(cart);

            sessionRequest.MerchantUrl = new MerchantUrl
            {
                Confirmation = $"{sessionRequest.MerchantUrl.Confirmation}?orderNumber={sessionRequest.MerchantReference1}&contactId={cart.CustomerId}",
            };

            if (ServiceLocator.Current.TryGetExistingInstance(out ISessionBuilder sessionBuilder))
            {
                sessionRequest = sessionBuilder.Build(sessionRequest, cart, config);
            }
            return await GetClient(cart.MarketId)
                .CreateOrder(authorizationToken, sessionRequest)
                .ConfigureAwait(false);
        }

        public CompletionResult Complete(IPurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null)
            {
                throw new ArgumentNullException(nameof(purchaseOrder));
            }
            var orderForm = purchaseOrder.GetFirstForm();
            var payment = orderForm?.Payments.FirstOrDefault(x => x.PaymentMethodName.Equals(Constants.KlarnaPaymentSystemKeyword));
            if (payment == null)
            {
                return CompletionResult.Empty;
            }

            SetOrderStatus(purchaseOrder, payment);

            var url = payment.Properties[Constants.KlarnaConfirmationUrlPaymentField]?.ToString();
            if (string.IsNullOrEmpty(url))
            {
                return CompletionResult.Empty;
            }

            return CompletionResult.WithRedirectUrl(url);
        }

        public bool CanSendPersonalInformation(string countryCode)
        {
            var continent = CountryCodeHelper.GetContinentByCountry(countryCode);

            return !continent.Equals("EU", StringComparison.InvariantCultureIgnoreCase);
        }

        public bool AllowedToSharePersonalInformation(ICart cart)
        {
            if (bool.TryParse(
                cart.Properties[Constants.KlarnaAllowSharingOfPersonalInformationCartField]?.ToString() ?? "false",
                out var canSendPersonalInformation))
            {
                return canSendPersonalInformation;
            }
            return false;
        }

        public bool AllowSharingOfPersonalInformation(ICart cart)
        {
            try
            {
                cart.Properties[Constants.KlarnaAllowSharingOfPersonalInformationCartField] = true;
                _orderRepository.Save(cart);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
            return false;
        }

        public PersonalInformationSession GetPersonalInformationSession(ICart cart, IDictionary<string, object> dic = null)
        {
            var request = new Session();

            var shipment = cart.GetFirstShipment();
            var payment = cart.GetFirstForm()?.Payments.FirstOrDefault();

            if (shipment != null && shipment.ShippingAddress != null)
            {
                request.ShippingAddress = shipment.ShippingAddress.ToAddress();
            }
            if (payment != null && payment.BillingAddress != null)
            {
                request.BillingAddress = payment.BillingAddress.ToAddress();
            }

            if (ServiceLocator.Current.TryGetExistingInstance(out ISessionBuilder sessionBuilder))
            {
                var config = _configurationLoader.GetPaymentsConfiguration(cart.MarketId);
                request = sessionBuilder.Build(request, cart, config, dic, true);
            }

            return new PersonalInformationSession
            {
                Customer = request.Customer,
                BillingAddress = request.BillingAddress,
                ShippingAddress = request.ShippingAddress
            };
        }

        private void SetOrderStatus(IPurchaseOrder purchaseOrder, IPayment payment)
        {
            if (payment.HasFraudStatus(FraudStatus.PENDING))
            {
                _purchaseOrderProcessor.HoldOrder(purchaseOrder);
                _orderRepository.Save(purchaseOrder);
            }
        }

        private Session GetSessionRequest(ICart cart, PaymentsConfiguration config, Uri siteUrl, bool includePersonalInformation = false)
        {
            var market = _marketService.GetMarket(cart.MarketId);
            var totals = _orderGroupCalculator.GetOrderGroupTotals(cart);
            var request = new Session
            {
                PurchaseCountry = CountryCodeHelper.GetTwoLetterCountryCode(market.Countries.FirstOrDefault()),
                OrderAmount = AmountHelper.GetAmount(totals.Total),
                // Non-negative, minor units. The total tax amount of the order.
                OrderTaxAmount = AmountHelper.GetAmount(totals.TaxTotal),
                PurchaseCurrency = cart.Currency.CurrencyCode,
                Locale = _languageService.GetPreferredCulture().Name,
                OrderLines = GetOrderLines(cart, totals, config.SendProductAndImageUrl).ToArray()
            };

            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(
                Constants.KlarnaPaymentSystemKeyword, _languageService.GetPreferredCulture().Name, returnInactive: true);
            if (paymentMethod != null)
            {
                request.MerchantUrl = new MerchantUrl
                {
                    Confirmation = ToFullSiteUrl(siteUrl, config.ConfirmationUrl),
                    Notification = ToFullSiteUrl(siteUrl, config.NotificationUrl),
                    Push = ToFullSiteUrl(siteUrl, config.PushUrl),
                };
                request.Options = new Options
                {
                    ColorBorder = config.WidgetBorderColor,
                    ColorBorderSelected = config.WidgetSelectedBorderColor,
                    ColorDetails = config.WidgetDetailsColor,
                    ColorText = config.WidgetTextColor,
                    RadiusBorder = config.WidgetBorderRadius
                };
                request.AutoCapture = config.AutoCapture;
                request.Design = config.Design;
            }

            if (includePersonalInformation)
            {
                var shipment = cart.GetFirstShipment();
                var payment = cart.GetFirstForm()?.Payments.FirstOrDefault();

                if (shipment?.ShippingAddress != null)
                {
                    request.ShippingAddress = shipment.ShippingAddress.ToAddress();
                }
                if (payment?.BillingAddress != null)
                {
                    request.BillingAddress = payment.BillingAddress.ToAddress();
                }
                else if (request.ShippingAddress != null)
                {
                    request.BillingAddress = new OrderManagementAddressInfo()
                    {
                        Email = request.ShippingAddress?.Email,
                        Phone = request.ShippingAddress?.Phone
                    };
                }

            }
            return request;
        }

        private string ToFullSiteUrl(Uri siteUrl, string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var absolute))
            {
                return absolute.ToString();
            }

            if (siteUrl == null)
            {
                siteUrl = SiteUrlHelper.GetCurrentSiteUrl();
            }

            return new Uri(siteUrl, url).ToString();
        }

        private async Task<bool> CreateSession(Session sessionRequest, ICart cart, SessionSettings settings)
        {
            try
            {
                var response = await GetClient(cart.MarketId)
                    .CreateSession(sessionRequest)
                    .ConfigureAwait(false);

                cart.SetKlarnaSessionId(response.SessionId);
                cart.SetKlarnaClientToken(response.ClientToken);
                cart.SetKlarnaPaymentMethodCategories(response.PaymentMethodCategories);
                cart.SetKlarnaPaymentsDescriptor(response.Descriptor);
                cart.SetSiteUrl(settings.SiteUrl);

                _orderRepository.Save(cart);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
            return false;
        }

        public virtual PaymentsStore GetClient(MarketId marketId)
        {
            var config = _configurationLoader.GetPaymentsConfiguration(marketId);

            string userAgent = $"Platform/Episerver.Commerce_{typeof(EPiServer.Commerce.ApplicationContext).Assembly.GetName().Version} Module/Klarna.Payments_{typeof(KlarnaPaymentsService).Assembly.GetName().Version}";

            return new PaymentsStore(new ApiSession
            {
                ApiUrl = config.ApiUrl,
                UserAgent = userAgent,
                Credentials = new ApiCredentials
                {
                    Username = config.Username,
                    Password = config.Password
                }
            }, new JsonSerializer());
        }
    }
}

