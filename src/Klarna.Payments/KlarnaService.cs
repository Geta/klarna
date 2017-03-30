using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Klarna.Payments.Models;
using Mediachase.Commerce;
using EPiServer.Logging;
using Klarna.Payments.Extensions;
using Mediachase.Commerce.Orders.Managers;

namespace Klarna.Payments
{
    [ServiceConfiguration(typeof(IKlarnaService))]
    public class KlarnaService : IKlarnaService
    {
        private readonly IKlarnaServiceApi _klarnaServiceApi;
        private readonly IOrderGroupTotalsCalculator _orderGroupTotalsCalculator;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(KlarnaService));
        private readonly IOrderRepository _orderRepository;

        public KlarnaService(IKlarnaServiceApi klarnaServiceApi, IOrderGroupTotalsCalculator orderGroupTotalsCalculator, IOrderRepository orderRepository)
        {
            _klarnaServiceApi = klarnaServiceApi;
            _orderGroupTotalsCalculator = orderGroupTotalsCalculator;
            _orderRepository = orderRepository;
        }

        public async Task<string> CreateSession(Session request)
        {
            try
            {
                var response = await _klarnaServiceApi.CreatNewSession(request).ConfigureAwait(false);

                return response.ClientToken;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
            return string.Empty;
        }

        public async Task<string> CreateOrUpdateSession(ICart cart)
        {
            var request = GetRequest(cart);

            var sessionId = cart.Properties[Constants.KlarnaSessionIdField]?.ToString();
            if (!string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    await _klarnaServiceApi.UpdateSession(sessionId, request).ConfigureAwait(false);

                    return cart.Properties[Constants.KlarnaClientTokenField]?.ToString();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                }
            }
            return await CreateSession(cart);
        }

        public async Task<string> CreateSession(ICart cart)
        {
            var request = GetRequest(cart);

            try
            {
                var response = await _klarnaServiceApi.CreatNewSession(request).ConfigureAwait(false);

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

        public string GetClientToken(ICart cart)
        {
            return cart.Properties[Constants.KlarnaClientTokenField]?.ToString();
        }

        public async Task<Session> GetSession(string sessionId)
        {
            return await _klarnaServiceApi.GetSession(sessionId).ConfigureAwait(false);
        }

        public async Task<CreateOrderResponse> CreateOrder(string authorizationToken, ICart cart)
        {
            try
            {
                var sessionId = cart.Properties[Constants.KlarnaSessionIdField]?.ToString();
                if (!string.IsNullOrEmpty(sessionId))
                {
                    var session = await GetSession(sessionId);

                    return await _klarnaServiceApi.CreateOrder(authorizationToken, session).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
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
                _logger.Error(ex.Message);
            }
        }

        public WidgetColorOptions GetWidgetColorOptions()
        {
            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaPaymentSystemKeyword, ContentLanguage.PreferredCulture.Name);
            if (paymentMethod == null)
            {
                return new WidgetColorOptions();
            }
            return WidgetColorOptions.FromPaymentMethod(paymentMethod);
        }

        private Session GetRequest(ICart cart)
        {
            var request = new Session();

            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaPaymentSystemKeyword, ContentLanguage.PreferredCulture.Name);
            if (paymentMethod != null)
            {
                request.MerchantUrl = new MerchantUrl
                {
                    Confirmation = paymentMethod.GetParameter(Constants.ConfirmationUrlField),
                    Notification = paymentMethod.GetParameter(Constants.NotificationUrlField),
                };
                request.Options = new Options
                {
                    ColorDetails = paymentMethod.GetParameter(Constants.KlarnaWidgetColorDetailsField, "#C0FFEE"),
                    ColorButton = paymentMethod.GetParameter(Constants.KlarnaWidgetColorButtonField, "#C0FFEE"),
                    ColorButtonText = paymentMethod.GetParameter(Constants.KlarnaWidgetColorButtonTextField, "#C0FFEE"),
                    ColorCheckbox = paymentMethod.GetParameter(Constants.KlarnaWidgetColorCheckboxField, "#C0FFEE"),
                    ColorCheckboxCheckmark = paymentMethod.GetParameter(Constants.KlarnaWidgetColorCheckboxCheckmarkField, "#C0FFEE"),
                    ColorHeader = paymentMethod.GetParameter(Constants.KlarnaWidgetColorHeaderField, "#C0FFEE"),
                    ColorLink = paymentMethod.GetParameter(Constants.KlarnaWidgetColorLinkField, "#C0FFEE"),
                    ColorBorder = paymentMethod.GetParameter(Constants.KlarnaWidgetColorBorderField, "#C0FFEE"),
                    ColorBorderSelected = paymentMethod.GetParameter(Constants.KlarnaWidgetColorBorderSelectedField, "#C0FFEE"),
                    ColorText = paymentMethod.GetParameter(Constants.KlarnaWidgetColorTextField, "#C0FFEE"),
                    ColorTextSecondary = paymentMethod.GetParameter(Constants.KlarnaWidgetColorTextSecondaryField, "#C0FFEE"),
                    RadiusBorder = paymentMethod.GetParameter(Constants.KlarnaWidgetRadiusBorderField, "#0px")
                };
            }

            request.BillingAddress = new Address
            {
                Email = "john.doe+red@abcstore.com",
                Phone = "202-555-0174",
                Country = "US",
                City = "New York"
            };

            var totals = _orderGroupTotalsCalculator.GetTotals(cart);

            var shipment = cart.GetFirstShipment();

            request.OrderAmount = GetAmount(totals.SubTotal);
            // request.OrderTaxAmount = GetAmount(totals.TaxTotal);

            request.PurchaseCurrency = cart.Currency.CurrencyCode;
            request.PurchaseCountry = GetTwoLetterCountryCode(shipment.ShippingAddress.CountryCode);
            request.Locale = ContentLanguage.PreferredCulture.Name;

            var list = new List<OrderLine>();
            foreach (var item in cart.GetAllLineItems())
            {
                var orderLine = new OrderLine();
                orderLine.Quantity = (int)item.Quantity;
                orderLine.Name = item.DisplayName;
                orderLine.Reference = item.Code;
                orderLine.UnitPrice = GetAmount(item.PlacedPrice);
                orderLine.TotalDiscountAmount = orderLine.UnitPrice - GetAmount(item.GetDiscountedPrice(cart.Currency));
                orderLine.TotalAmount = orderLine.UnitPrice - orderLine.TotalDiscountAmount;

                list.Add(orderLine);
            }
            request.OrderLines = list.ToArray();


            return request;
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

        private string GetTwoLetterCountryCode(string code)
        {
            return ISO3166.Country.List.FirstOrDefault(x => x.ThreeLetterCode.Equals(code, StringComparison.InvariantCultureIgnoreCase))?.TwoLetterCode;
        }
    }
}

