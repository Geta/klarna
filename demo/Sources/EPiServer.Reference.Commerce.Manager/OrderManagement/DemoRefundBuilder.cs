using EPiServer.Commerce.Order;
using Klarna.OrderManagement.Refunds;
using Klarna.Rest.Models;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Manager.OrderManagement
{
    public class DemoRefundBuilder : RefundBuilder
    {
        public override Refund Build(Refund refund, IOrderGroup orderGroup, OrderForm returnOrderForm, IPayment payment)
        {
            return refund;
        }
    }
}