using EPiServer.Commerce.Order;
using Klarna.Rest.Core.Model;

namespace Klarna.OrderManagement
{
    public interface ICaptureBuilder
    {
        OrderManagementCapture Build(OrderManagementCapture captureData, IOrderGroup orderGroup, IOrderForm returnOrderForm, IPayment payment);
    }
}
