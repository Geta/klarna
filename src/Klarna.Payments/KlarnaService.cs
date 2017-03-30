using System.Collections.Generic;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Klarna.Payments.Models;

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

        public async Task<string> CreateSession(ICart cart)
        {
            var request = new CreateSessionRequest();
            
            var totals = _orderGroupTotalsCalculator.GetTotals(cart);

            request.OrderAmount = (int) totals.Total.Amount;
            request.OrderTaxAmount = (int)totals.TaxTotal.Amount;

            request.PurchaseCurrency = cart.Currency.CurrencyCode;
            request.PurchaseCountry = "US";
            request.Locale = "en-us";

            var list = new List<OrderLines>();
            foreach (var item in cart.GetAllLineItems())
            {
                var orderLine = new OrderLines();
                orderLine.Quantity = (int)item.Quantity;
                orderLine.Name = item.DisplayName;
                orderLine.Reference = item.Code;
                orderLine.TotalAmount = orderLine.UnitPrice = (int) item.PlacedPrice;
                
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
    }
}
