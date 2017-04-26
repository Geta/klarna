using EPiServer.Commerce.Order;
using Klarna.Rest.Models;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement.Refunds
{
    public abstract class RefundBuilder
    {
        public abstract Refund Build(Refund refund, IOrderGroup orderGroup, OrderForm returnOrderForm, IPayment payment);
    }
}
