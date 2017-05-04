using EPiServer.Commerce.Order;
using Klarna.Rest.Models;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement.Refunds
{
    public interface IRefundBuilder
    {
        Refund Build(Refund refund, IOrderGroup orderGroup, OrderForm returnOrderForm, IPayment payment);
    }
}
