using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Klarna.Payments.Models;
using EPiServer.Logging;
using Klarna.Common;
using Klarna.Common.Extensions;
using Klarna.Common.Helpers;
using Klarna.Payments.Extensions;
using Klarna.Rest.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Refit;

namespace Klarna.Payments
{
    [ServiceConfiguration(typeof(IKlarnaPaymentsService))]
    public class KlarnaPaymentsService : KlarnaService, IKlarnaPaymentsService
    {
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(KlarnaPaymentsService));
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly KlarnaServiceApiFactory _klarnaServiceApiFactory;

        public KlarnaPaymentsService(
            IOrderRepository orderRepository,
            IOrderNumberGenerator orderNumberGenerator,
            IPaymentProcessor paymentProcessor,
            IOrderGroupCalculator orderGroupCalculator,
            KlarnaServiceApiFactory klarnaServiceApiFactory)
            : base(orderRepository, paymentProcessor, orderGroupCalculator)
        {
            _orderGroupCalculator = orderGroupCalculator;
            _orderRepository = orderRepository;
            _orderNumberGenerator = orderNumberGenerator;
            _klarnaServiceApiFactory = klarnaServiceApiFactory;
        }

        public async Task<bool> CreateOrUpdateSession(ICart cart, IDictionary<string, object> dic = null)
        {
            // Check if we shared PI before, if so it allows us to share it again
            var canSendPersonalInformation = AllowedToSharePersonalInformation(cart);
            var config = GetConfiguration(cart.Market.MarketId);

            var sessionRequest = GetSessionRequest(cart, config, canSendPersonalInformation);
            if (ServiceLocator.Current.TryGetExistingInstance(out ISessionBuilder sessionBuilder))
            {
                sessionRequest = sessionBuilder.Build(sessionRequest, cart, config, dic);
            }
            var currentCountry = cart.Market.Countries.FirstOrDefault();
            // Clear PI if we're not allowed to send it yet (can be set by custom session builder)
            if (!canSendPersonalInformation && !CanSendPersonalInformation(currentCountry))
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
            var sessionId = cart.Properties[Constants.KlarnaSessionIdCartField]?.ToString();
            if (!string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    await _klarnaServiceApiFactory.Create(GetConfiguration(cart.Market))
                        .UpdateSession(sessionId, sessionRequest)
                        .ConfigureAwait(false);

                    return true;
                }
                catch (ApiException apiException)
                {
                    // Create new session if current one is not found
                    if (apiException.StatusCode == HttpStatusCode.NotFound)
                    {
                        return await CreateSession(sessionRequest, cart);
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
            return await CreateSession(sessionRequest, cart);
        }

        public string GetSessionId(ICart cart)
        {
            return cart.Properties[Constants.KlarnaSessionIdCartField]?.ToString();
        }

        public string GetClientToken(ICart cart)
        {
            return cart.Properties[Constants.KlarnaClientTokenCartField]?.ToString();
        }

        public async Task<Session> GetSession(ICart cart)
        {
            return await _klarnaServiceApiFactory.Create(GetConfiguration(cart.Market))
                .GetSession(GetSessionId(cart))
                .ConfigureAwait(false);
        }

        public async Task<CreateOrderResponse> CreateOrder(string authorizationToken, ICart cart)
        {
            var config = GetConfiguration(cart.Market.MarketId);
            var sessionRequest = GetSessionRequest(cart, config, true);

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
            return await _klarnaServiceApiFactory.Create(GetConfiguration(cart.Market))
                .CreateOrder(authorizationToken, sessionRequest)
                .ConfigureAwait(false);
        }

        public async Task CancelAuthorization(string authorizationToken, IMarket market)
        {
            try
            {
                await _klarnaServiceApiFactory.Create(GetConfiguration(market))
                    .CancelAuthorization(authorizationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }

        public void CompleteAndRedirect(IPurchaseOrder purchaseOrder)
        {
            var result = Complete(purchaseOrder);
            if (result.IsRedirect)
            {
                HttpContext.Current.Response.Redirect(result.RedirectUrl);
            }
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
                var config = GetConfiguration(cart.Market.MarketId);
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
            var fraudStatus = payment.Properties[Common.Constants.FraudStatusPaymentField]?.ToString();
            if (fraudStatus == FraudStatus.PENDING.ToString())
            {
                OrderStatusManager.HoldOrder((PurchaseOrder)purchaseOrder);
                _orderRepository.Save(purchaseOrder);
            }
        }

        private Session GetSessionRequest(ICart cart, PaymentsConfiguration config, bool includePersonalInformation = false)
        {
            var totals = _orderGroupCalculator.GetOrderGroupTotals(cart);
            var request = new Session
            {
                PurchaseCountry = CountryCodeHelper.GetTwoLetterCountryCode(cart.Market.Countries.FirstOrDefault()),
                OrderAmount = AmountHelper.GetAmount(totals.Total),
                // Non-negative, minor units. The total tax amount of the order.
                OrderTaxAmount = AmountHelper.GetAmount(totals.TaxTotal),
                PurchaseCurrency = cart.Currency.CurrencyCode,
                Locale = ContentLanguage.PreferredCulture.Name,
                OrderLines = GetOrderLines(cart, totals, config.SendProductAndImageUrlField).ToArray()
            };

            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaPaymentSystemKeyword, ContentLanguage.PreferredCulture.Name);
            if (paymentMethod != null)
            {
                request.MerchantUrl = new MerchantUrl
                {
                    Confirmation = config.ConfirmationUrl,
                    Notification = config.NotificationUrl,
                };
                request.Options = GetWidgetOptions(paymentMethod, cart.Market.MarketId);
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
                    request.BillingAddress = new Address()
                    {
                        Email = request.ShippingAddress?.Email,
                        Phone = request.ShippingAddress?.Phone
                    };
                }

            }
            return request;
        }

        private async Task<bool> CreateSession(Session sessionRequest, ICart cart)
        {
            try
            {
                var response = await _klarnaServiceApiFactory.Create(GetConfiguration(cart.Market))
                    .CreatNewSession(sessionRequest)
                    .ConfigureAwait(false);

                cart.Properties[Constants.KlarnaSessionIdCartField] = response.SessionId;
                cart.Properties[Constants.KlarnaClientTokenCartField] = response.ClientToken;

                _orderRepository.Save(cart);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
            return false;
        }

        private Options GetWidgetOptions(PaymentMethodDto paymentMethod, MarketId marketId)
        {
            var configuration = paymentMethod.GetKlarnaPaymentsConfiguration(marketId);
            var options = new Options();

            options.ColorDetails = GetString(configuration.WidgetDetailsColor);
            options.ColorButton = GetString(configuration.WidgetButtonColor);
            options.ColorButtonText = GetString(configuration.WidgetButtonTextColor);
            options.ColorCheckbox = GetString(configuration.WidgetCheckboxColor);
            options.ColorCheckboxCheckmark = GetString(configuration.WidgetCheckboxCheckmarkColor);
            options.ColorHeader = GetString(configuration.WidgetHeaderColor);
            options.ColorLink = GetString(configuration.WidgetLinkColor);
            options.ColorBorder = GetString(configuration.WidgetBorderColor);
            options.ColorBorderSelected = GetString(configuration.WidgetSelectedBorderColor);
            options.ColorText = GetString(configuration.WidgetTextColor);
            options.ColorTextSecondary = GetString(configuration.WidgetTextSecondaryColor);
            options.RadiusBorder = GetString(configuration.WidgetBorderRadius);

            return options;
        }
        private string GetString(string input)
        {
            return string.IsNullOrWhiteSpace(input) ? null : input;
        }

        public PaymentsConfiguration GetConfiguration(IMarket market)
        {
            return GetConfiguration(market.MarketId);
        }

        public PaymentsConfiguration GetConfiguration(MarketId marketId)
        {
            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaPaymentSystemKeyword, ContentLanguage.PreferredCulture.Name);
            if (paymentMethod == null)
            {
                throw new Exception(
                    $"PaymentMethod {Constants.KlarnaPaymentSystemKeyword} is not configured for market {marketId} and language {ContentLanguage.PreferredCulture.Name}");
            }
            return paymentMethod.GetKlarnaPaymentsConfiguration(marketId);
        }
    }
}

