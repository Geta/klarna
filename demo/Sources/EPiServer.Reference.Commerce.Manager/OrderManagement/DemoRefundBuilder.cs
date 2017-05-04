using EPiServer.Commerce.Order;
using Klarna.OrderManagement.Refunds;
using Klarna.Rest.Models;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Manager.OrderManagement
{
    public class DemoRefundBuilder : IRefundBuilder
    {
        public Refund Build(Refund refund, IOrderGroup orderGroup, OrderForm returnOrderForm, IPayment payment)
        {
            // Here you can make changes to refund if needed
            return refund;
        }
    }
}