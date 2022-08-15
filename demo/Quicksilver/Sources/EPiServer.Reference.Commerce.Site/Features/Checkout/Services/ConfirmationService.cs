using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Search;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Services
{
    public class ConfirmationService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentMarket _currentMarket;

        public ConfirmationService(
            IOrderRepository orderRepository,
            ICurrentMarket currentMarket)
        {
            _orderRepository = orderRepository;
            _currentMarket = currentMarket;
        }

        public IPurchaseOrder GetOrder(int orderNumber)
        {
            return _orderRepository.Load<IPurchaseOrder>(orderNumber);
        }

        public IPurchaseOrder CreateFakePurchaseOrder()
        {
            var form = new InMemoryOrderForm
            {
                Payments =
                {
                    new InMemoryPayment
                    {
                        BillingAddress = new InMemoryOrderAddress(),
                        PaymentMethodName = "CashOnDelivery"
                    }
                }
            };

            form.Shipments.First().ShippingAddress = new InMemoryOrderAddress();
            var market = _currentMarket.GetCurrentMarket();
            var purchaseOrder = new InMemoryPurchaseOrder
            {
                Currency = _currentMarket.GetCurrentMarket().DefaultCurrency,
                MarketId = market.MarketId,
                MarketName = market.MarketName,
                PricesIncludeTax = market.PricesIncludeTax,
                OrderLink = new OrderReference(0, string.Empty, Guid.Empty, typeof(IPurchaseOrder))
            };
            purchaseOrder.Forms.Add(form);

            return purchaseOrder;
        }

        public IPurchaseOrder GetByTrackingNumber(string trackingNumber)
        {
            OrderSearchOptions searchOptions = new OrderSearchOptions();
            searchOptions.CacheResults = false;
            searchOptions.StartingRecord = 0;
            searchOptions.RecordsToRetrieve = 1;
            searchOptions.Classes = new System.Collections.Specialized.StringCollection { "PurchaseOrder" };
            searchOptions.Namespace = "Mediachase.Commerce.Orders";

            var parameters = new OrderSearchParameters();
            parameters.SqlMetaWhereClause = $"META.TrackingNumber = '{trackingNumber}'";

            var purchaseOrder = OrderContext.Current.FindPurchaseOrders(parameters, searchOptions)?.FirstOrDefault();

            if (purchaseOrder != null)
            {
                return _orderRepository.Load<IPurchaseOrder>(purchaseOrder.OrderGroupId);
            }
            return null;
        }
    }
}