using EPiServer.Commerce.Order;
using Klarna.Common.Models;

namespace Klarna.OrderManagement
{
    public interface ICaptureBuilder
    {
        OrderManagementCapture Build(OrderManagementCapture captureData, IOrderGroup orderGroup, IOrderForm returnOrderForm, IPayment payment);
    }
}
