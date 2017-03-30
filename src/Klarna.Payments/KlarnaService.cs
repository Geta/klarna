using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Klarna.Payments.Models;
using Mediachase.Commerce;

namespace Klarna.Payments
{
    [ServiceConfiguration(typeof(KlarnaService))]
    public class KlarnaService
    {
        private readonly IKlarnaServiceApi _klarnaServiceApi;
        private readonly IOrderGroupTotalsCalculator _orderGroupTotalsCalculator;

        public KlarnaService(IKlarnaServiceApi klarnaServiceApi, IOrderGroupTotalsCalculator orderGroupTotalsCalculator)
        {
            _klarnaServiceApi = klarnaServiceApi;
            _orderGroupTotalsCalculator = orderGroupTotalsCalculator;
        }

        public async Task<string> CreateSession(CreateSessionRequest request)
        {
            var response = await _klarnaServiceApi.CreatNewSession(request).ConfigureAwait(false);

            return response.ClientToken;
        }

        public async Task<string> CreateSession(ICart cart)
        {
            var request = new CreateSessionRequest();
            
            var totals = _orderGroupTotalsCalculator.GetTotals(cart);

            var shipment = cart.GetFirstShipment();

            request.OrderAmount = GetAmount(totals.SubTotal);
           // request.OrderTaxAmount = GetAmount(totals.TaxTotal);

            request.PurchaseCurrency = cart.Currency.CurrencyCode;
            request.PurchaseCountry = GetTwoLetterCountryCode(shipment.ShippingAddress.CountryCode);
            request.Locale = ContentLanguage.PreferredCulture.Name;

            var list = new List<OrderLines>();
            foreach (var item in cart.GetAllLineItems())
            {
                var orderLine = new OrderLines();
                orderLine.Quantity = (int)item.Quantity;
                orderLine.Name = item.DisplayName;
                orderLine.Reference = item.Code;
                orderLine.UnitPrice = GetAmount(item.PlacedPrice);
                orderLine.TotalDiscountAmount = orderLine.UnitPrice - GetAmount(item.GetDiscountedPrice(cart.Currency));
                orderLine.TotalAmount = orderLine.UnitPrice - orderLine.TotalDiscountAmount;

                list.Add(orderLine);
            }
            request.OrderLines = list.ToArray();

            var response = await _klarnaServiceApi.CreatNewSession(request).ConfigureAwait(false);

            return response.ClientToken;
        }

        public async Task<CreateSessionRequest> GetSession(string sessionId)
        {
            return await _klarnaServiceApi.GetSession(sessionId);
        }

        public WidgetColorOptions GetWidgetColorOptions()
        {
            return new WidgetColorOptions();
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
