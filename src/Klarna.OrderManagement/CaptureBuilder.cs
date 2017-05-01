using EPiServer.Commerce.Order;
using Klarna.Rest.Models;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement
{
    public abstract class CaptureBuilder
    {
        public abstract CaptureData Build(CaptureData captureData, IOrderGroup orderGroup, IOrderForm returnOrderForm, IPayment payment);
    }
}
