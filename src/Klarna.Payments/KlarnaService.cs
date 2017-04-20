using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Klarna.Payments.Models;
using Mediachase.Commerce;
using EPiServer.Logging;
using EPiServer.Web.Routing;
using Klarna.Payments.Extensions;
using Klarna.Payments.Helpers;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Orders.Search;

namespace Klarna.Payments
{
    [ServiceConfiguration(typeof(IKlarnaService))]
    public class KlarnaService : IKlarnaService
    {
        private readonly IKlarnaServiceApi _klarnaServiceApi;
        private readonly IOrderGroupTotalsCalculator _orderGroupTotalsCalculator;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(KlarnaService));
        private readonly IOrderRepository _orderRepository;
        private readonly ReferenceConverter _referenceConverter;
        private readonly UrlResolver _urlResolver;
        private readonly IContentRepository _contentRepository;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IOrderGroupCalculator _orderGroupCalculator;

        private Configuration _configuration;

        public KlarnaService( 
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

        public async Task<string> CreateOrUpdateSession(Session sessionRequest, ICart cart)
        {
            // If the pre assessment is not enabled then don't send the customer information to Klarna
            if (!Configuration.IsCustomerPreAssessmentEnabled || !CanSendPersonalInformation(cart.Market.Countries.FirstOrDefault()))
            {
                sessionRequest.Customer = null;
            }
            var sessionId = cart.Properties[Constants.KlarnaSessionIdField]?.ToString();
            if (!string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    await _klarnaServiceApi.UpdateSession(sessionId, sessionRequest).ConfigureAwait(false);

                    return cart.Properties[Constants.KlarnaClientTokenField]?.ToString();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex);
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
            throw new Exception("See TODO");

            try
            {
                // TODO: REMOVE - We should not do this...
                // Retrieving session from klarna in order to create an order is unsafe, user can change data during authorization
                // However have to check if we need to provide personal info during create order or just cart info
                var session = await GetSession(cart);

                session.MerchantReference1 = _orderNumberGenerator.GenerateOrderNumber(cart);
                session.MerchantUrl = new MerchantUrl
                    {
                        Confirmation = $"{session.MerchantUrl.Confirmation}?trackingNumber={session.MerchantReference1}",
                    };
                return await _klarnaServiceApi.CreateOrder(authorizationToken, session).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            return null;
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
        
        public virtual Session GetSessionRequest(ICart cart)
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
            request.OrderAmount = GetAmount(totals.Total);
            request.PurchaseCurrency = cart.Currency.CurrencyCode;
            request.Locale = ContentLanguage.PreferredCulture.Name;
            
            var list = new List<OrderLine>();
            foreach (var item in cart.GetAllLineItems())
            {
                var orderLine = GetOrderLine(item, cart.Currency);

                list.Add(orderLine);
            }
            if (totals.ShippingTotal.Amount > 0)
            {
                list.Add(new OrderLine
                {
                    Name = "Shipping method",
                    Quantity = 1,
                    UnitPrice = GetAmount(totals.ShippingTotal.Amount),
                    TotalAmount = GetAmount(totals.ShippingTotal.Amount)
                });
            }
            request.OrderLines = list.ToArray();
            
            return request;
        }

        public virtual PersonalInformationSession GetPersonalInformationSession(ICart cart)
        {
            var request = new PersonalInformationSession();
            if (Configuration.IsCustomerPreAssessmentEnabled)
            {
                request.Customer = new Customer
                {
                    DateOfBirth = "1980-01-01",
                    Gender = "Male",
                    LastFourSsn = "1234"
                };
            }

            var shipment = cart.GetFirstShipment();
            if (shipment?.ShippingAddress != null)
            {
                request.ShippingAddress = shipment.ShippingAddress.ToAddress();
            }

            return request;
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

        private async Task<string> CreateSession(Session sessionRequest, ICart cart)
        {
            try
            {
                var response = await _klarnaServiceApi.CreatNewSession(sessionRequest).ConfigureAwait(false);

                cart.Properties[Constants.KlarnaSessionIdField] = response.SessionId;
                cart.Properties[Constants.KlarnaClientTokenField] = response.ClientToken;

                _orderRepository.Save(cart);

                return response.ClientToken;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
            return string.Empty;
        }

        private int GetAmount(decimal money)
        {
            if (money > 0)
            {
                return (int)(money * 100);
            }
            return 0;
        }

        private int GetAmount(Money money)
        {
            if (money.Amount > 0)
            {
                return (int)(money.Amount * 100);
            }
            return 0;
        }

        private string GetVariantImage(ContentReference contentReference)
        {
            VariationContent variant;
            if (_contentRepository.TryGet(contentReference, out variant))
            {
                return variant.CommerceMediaCollection.Select(media => _urlResolver.GetUrl(media.AssetLink)).FirstOrDefault();
            }
            return string.Empty;
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

        private OrderLine GetOrderLine(ILineItem item, Currency currency)
        {
            var orderLine = new OrderLine();
            orderLine.Quantity = (int)item.Quantity;
            orderLine.Name = item.DisplayName;
            orderLine.Reference = item.Code;
            orderLine.UnitPrice = GetAmount(item.PlacedPrice);
            orderLine.TotalDiscountAmount = GetAmount(item.GetEntryDiscount());
            orderLine.TotalAmount = GetAmount(item.GetExtendedPrice(currency).Amount);

            if (Configuration.SendProductAndImageUrlField)
            {
                var contentLink = _referenceConverter.GetContentLink(item.Code);
                if (!ContentReference.IsNullOrEmpty(contentLink))
                {
                    orderLine.ProductUrl = _urlResolver.GetUrl(contentLink);
                    orderLine.ProductImageUrl = GetVariantImage(contentLink);
                }
            }
            return orderLine;
        }

        private Configuration GetConfiguration()
        {
            var configuration = new Configuration();

            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaPaymentSystemKeyword, ContentLanguage.PreferredCulture.Name);
            if (paymentMethod != null)
            {
                configuration.IsCustomerPreAssessmentEnabled = bool.Parse(paymentMethod.GetParameter(Constants.PreAssesmentField, "false"));
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
            parameters.SqlMetaWhereClause = $"META.{Constants.KlarnaOrderIdField} LIKE '{orderId}'";

            var purchaseOrder = OrderContext.Current.FindPurchaseOrders(parameters, searchOptions)?.FirstOrDefault();

            if (purchaseOrder != null)
            {
                return _orderRepository.Load<IPurchaseOrder>(purchaseOrder.OrderGroupId);
            }
            return null;
        }
    }
}

