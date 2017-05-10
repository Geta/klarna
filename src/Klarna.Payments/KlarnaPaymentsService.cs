using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using EPiServer;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Klarna.Payments.Models;
using EPiServer.Logging;
using EPiServer.Web.Routing;
using Klarna.Common.Extensions;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Klarna.Payments.Extensions;
using Klarna.Payments.Helpers;
using Klarna.Rest.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Orders.Search;
using Refit;

namespace Klarna.Payments
{
    [ServiceConfiguration(typeof(IKlarnaPaymentsService))]
    public class KlarnaPaymentsService : IKlarnaPaymentsService
    {
        private readonly IKlarnaServiceApi _klarnaServiceApi;
        private readonly IOrderGroupTotalsCalculator _orderGroupTotalsCalculator;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(KlarnaPaymentsService));
        private readonly IOrderRepository _orderRepository;
        private readonly ReferenceConverter _referenceConverter;
        private readonly UrlResolver _urlResolver;
        private readonly IContentRepository _contentRepository;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IOrderGroupCalculator _orderGroupCalculator;

        private Configuration _configuration;

        public KlarnaPaymentsService(
            IOrderGroupTotalsCalculator orderGroupTotalsCalculator,
            IOrderRepository orderRepository,
            ReferenceConverter referenceConverter,
            UrlResolver urlResolver,
            IContentRepository contentRepository,
            IOrderNumberGenerator orderNumberGenerator,
            IPaymentProcessor paymentProcessor,
            IOrderGroupCalculator orderGroupCalculator)
        {
            _klarnaServiceApi = ServiceLocator.Current.GetInstance<IKlarnaServiceApi>();
            _orderGroupTotalsCalculator = orderGroupTotalsCalculator;
            _orderRepository = orderRepository;
            _referenceConverter = referenceConverter;
            _urlResolver = urlResolver;
            _contentRepository = contentRepository;
            _orderNumberGenerator = orderNumberGenerator;
            _paymentProcessor = paymentProcessor;
            _orderGroupCalculator = orderGroupCalculator;
        }

        public Configuration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = GetConfiguration();
                }
                return _configuration;
            }
        }

        public async Task<bool> CreateOrUpdateSession(ICart cart)
        {
            // Check if we shared PI before, if so it allows us to share it again
            var canSendPersonalInformation = AllowedToSharePersonalInformation(cart);

            var sessionRequest = GetSessionRequest(cart, canSendPersonalInformation);
            if (ServiceLocator.Current.TryGetExistingInstance(out ISessionBuilder sessionBuilder))
            {
                sessionRequest = sessionBuilder.Build(sessionRequest, cart, Configuration);
            }

            var currentCountry = cart.Market.Countries.FirstOrDefault();
            // Clear PI if we're not allowed to send it yet (can be set by custom session builder)
            if (!canSendPersonalInformation && !CanSendPersonalInformation(currentCountry))
            {
                // Can't share PI yet, will be done during the first authorize call
                sessionRequest.ShippingAddress = null;
                sessionRequest.BillingAddress = null;

                // If the pre assessment is not enabled then don't send the customer information to Klarna
                if (!Configuration.CustomerPreAssessmentCountries.Any(c => cart.Market.Countries.Contains(c)))
                {
                    sessionRequest.Customer = null;
                }
            }
            var sessionId = cart.Properties[Constants.KlarnaSessionIdField]?.ToString();
            if (!string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    await _klarnaServiceApi.UpdateSession(sessionId, sessionRequest).ConfigureAwait(false);

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
            return cart.Properties[Constants.KlarnaSessionIdField]?.ToString();
        }

        public string GetClientToken(ICart cart)
        {
            return cart.Properties[Constants.KlarnaClientTokenField]?.ToString();
        }

        public async Task<Session> GetSession(ICart cart)
        {
            return await _klarnaServiceApi.GetSession(GetSessionId(cart)).ConfigureAwait(false);
        }

        public async Task<CreateOrderResponse> CreateOrder(string authorizationToken, ICart cart)
        {
            var sessionRequest = GetSessionRequest(cart, true);
            if (ServiceLocator.Current.TryGetExistingInstance(out ISessionBuilder sessionBuilder))
            {
                sessionRequest = sessionBuilder.Build(sessionRequest, cart, Configuration, true);
            }

            sessionRequest.MerchantReference1 = _orderNumberGenerator.GenerateOrderNumber(cart);
            sessionRequest.MerchantUrl = new MerchantUrl
            {
                Confirmation = $"{sessionRequest.MerchantUrl.Confirmation}?trackingNumber={sessionRequest.MerchantReference1}",
            };
            return await _klarnaServiceApi.CreateOrder(authorizationToken, sessionRequest).ConfigureAwait(false);
        }

        public async Task CancelAuthorization(string authorizationToken)
        {
            try
            {
                await _klarnaServiceApi.CancelAuthorization(authorizationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }

        public void FraudUpdate(NotificationModel notification)
        {
            var order = GetPurchaseOrderByKlarnaOrderId(notification.OrderId);
            if (order != null)
            {
                var orderForm = order.GetFirstForm();
                var payment = orderForm.Payments.FirstOrDefault();
                if (payment != null && payment.Status == PaymentStatus.Pending.ToString())
                {
                    payment.Properties[Constants.FraudStatusPaymentMethodField] = notification.Status.ToString();

                    try
                    {
                        order.ProcessPayments(_paymentProcessor, _orderGroupCalculator);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message, ex);
                    }
                    _orderRepository.Save(order);
                }
            }
        }

        public void RedirectToConfirmationUrl(IPurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null)
            {
                throw new ArgumentNullException(nameof(purchaseOrder));
            }
            var orderForm = purchaseOrder.GetFirstForm();
            if (orderForm != null)
            {
                var payment = orderForm.Payments.FirstOrDefault(x => x.PaymentMethodName.Equals(Constants.KlarnaPaymentSystemKeyword));
                if (payment != null)
                {
                    var url = payment.Properties[Constants.KlarnaConfirmationUrlField]?.ToString();
                    if (!string.IsNullOrEmpty(url))
                    {
                        HttpContext.Current.Response.Redirect(url);
                    }
                }
            }
        }

        public bool CanSendPersonalInformation(string countryCode)
        {
            var continent = CountryCodeHelper.GetContinentByCountry(countryCode);

            return !continent.Equals("EU", StringComparison.InvariantCultureIgnoreCase);
        }

        public bool AllowedToSharePersonalInformation(ICart cart)
        {
            if (bool.TryParse(
                cart.Properties[Constants.KlarnaAllowSharingOfPersonalInformationField]?.ToString() ?? "false",
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
                cart.Properties[Constants.KlarnaAllowSharingOfPersonalInformationField] = true;
                _orderRepository.Save(cart);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
            return false;
        }

        public PersonalInformationSession GetPersonalInformationSession(ICart cart)
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
                request = sessionBuilder.Build(request, cart, Configuration, true);
            }

            return new PersonalInformationSession
            {
                Customer = request.Customer,
                BillingAddress = request.BillingAddress,
                ShippingAddress = request.ShippingAddress
            };
        }

        private Session GetSessionRequest(ICart cart, bool includePersonalInformation = false)
        {
            var request = new Session();
            request.PurchaseCountry = CountryCodeHelper.GetTwoLetterCountryCode(cart.Market.Countries.FirstOrDefault());

            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaPaymentSystemKeyword, ContentLanguage.PreferredCulture.Name);
            if (paymentMethod != null)
            {
                request.MerchantUrl = new MerchantUrl
                {
                    Confirmation = paymentMethod.GetParameter(Constants.ConfirmationUrlField),
                    Notification = paymentMethod.GetParameter(Constants.NotificationUrlField),
                };
                request.Options = GetWidgetOptions(paymentMethod);
            }

            var totals = _orderGroupTotalsCalculator.GetTotals(cart);
            request.OrderAmount = AmountHelper.GetAmount(totals.Total);
            request.PurchaseCurrency = cart.Currency.CurrencyCode;
            request.Locale = ContentLanguage.PreferredCulture.Name;

            var shipment = cart.GetFirstShipment();

            var list = new List<OrderLine>();
            foreach (var item in cart.GetAllLineItems())
            {
                var orderLine = item.GetOrderLine(cart.Market, shipment, cart.Currency, Configuration.SendProductAndImageUrlField);

                list.Add(orderLine);
            }
            if (totals.ShippingTotal.Amount > 0)
            {
                // TODO: Shipping tax
                var shipmentOrderLine = shipment.GetOrderLine(totals,  new Money(0, cart.Currency));
                list.Add(shipmentOrderLine);
            }
            
            request.OrderLines = list.ToArray();

            if (includePersonalInformation)
            {
                var payment = cart.GetFirstForm()?.Payments.FirstOrDefault();

                if (shipment != null && shipment.ShippingAddress != null)
                {
                    request.ShippingAddress = shipment.ShippingAddress.ToAddress();
                }
                if (payment != null && payment.BillingAddress != null)
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
                var response = await _klarnaServiceApi.CreatNewSession(sessionRequest).ConfigureAwait(false);

                cart.Properties[Constants.KlarnaSessionIdField] = response.SessionId;
                cart.Properties[Constants.KlarnaClientTokenField] = response.ClientToken;

                _orderRepository.Save(cart);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
            return false;
        }

        

        private Options GetWidgetOptions(PaymentMethodDto paymentMethod)
        {
            var options = new Options();

            options.ColorDetails = paymentMethod.GetParameter(Constants.KlarnaWidgetColorDetailsField, "#C0FFEE");
            options.ColorButton = paymentMethod.GetParameter(Constants.KlarnaWidgetColorButtonField, "#C0FFEE");
            options.ColorButtonText = paymentMethod.GetParameter(Constants.KlarnaWidgetColorButtonTextField, "#C0FFEE");
            options.ColorCheckbox = paymentMethod.GetParameter(Constants.KlarnaWidgetColorCheckboxField, "#C0FFEE");
            options.ColorCheckboxCheckmark =
                paymentMethod.GetParameter(Constants.KlarnaWidgetColorCheckboxCheckmarkField, "#C0FFEE");
            options.ColorHeader = paymentMethod.GetParameter(Constants.KlarnaWidgetColorHeaderField, "#C0FFEE");
            options.ColorLink = paymentMethod.GetParameter(Constants.KlarnaWidgetColorLinkField, "#C0FFEE");
            options.ColorBorder = paymentMethod.GetParameter(Constants.KlarnaWidgetColorBorderField, "#C0FFEE");
            options.ColorBorderSelected = paymentMethod.GetParameter(Constants.KlarnaWidgetColorBorderSelectedField,
                "#C0FFEE");
            options.ColorText = paymentMethod.GetParameter(Constants.KlarnaWidgetColorTextField, "#C0FFEE");
            options.ColorTextSecondary = paymentMethod.GetParameter(Constants.KlarnaWidgetColorTextSecondaryField,
                "#C0FFEE");
            options.RadiusBorder = paymentMethod.GetParameter(Constants.KlarnaWidgetRadiusBorderField, "#0px");

            return options;
        }

        private Configuration GetConfiguration()
        {
            var configuration = new Configuration();

            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaPaymentSystemKeyword, ContentLanguage.PreferredCulture.Name);
            if (paymentMethod != null)
            {
                configuration.CustomerPreAssessmentCountries = paymentMethod.GetParameter(Constants.PreAssesmentCountriesField, string.Empty)?.Split(',');
                configuration.SendProductAndImageUrlField = bool.Parse(paymentMethod.GetParameter(Constants.SendProductAndImageUrlField, "false"));
                configuration.UseAttachments = bool.Parse(paymentMethod.GetParameter(Constants.UseAttachmentsField, "false"));
            }
            return configuration;
        }

        private IPurchaseOrder GetPurchaseOrderByKlarnaOrderId(string orderId)
        {
            OrderSearchOptions searchOptions = new OrderSearchOptions();
            searchOptions.CacheResults = false;
            searchOptions.StartingRecord = 0;
            searchOptions.RecordsToRetrieve = 1;
            searchOptions.Classes = new System.Collections.Specialized.StringCollection { "PurchaseOrder" };
            searchOptions.Namespace = "Mediachase.Commerce.Orders";

            var parameters = new OrderSearchParameters();
            parameters.SqlMetaWhereClause = $"META.{Common.Constants.KlarnaOrderIdField} LIKE '{orderId}'";

            var purchaseOrder = OrderContext.Current.FindPurchaseOrders(parameters, searchOptions)?.FirstOrDefault();

            if (purchaseOrder != null)
            {
                return _orderRepository.Load<IPurchaseOrder>(purchaseOrder.OrderGroupId);
            }
            return null;
        }
    }
}

