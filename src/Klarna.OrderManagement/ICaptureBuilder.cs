using EPiServer.Commerce.Order;
using Klarna.Rest.Models;

namespace Klarna.OrderManagement
{
    public interface ICaptureBuilder
    {
        CaptureData Build(CaptureData captureData, IOrderGroup orderGroup, IOrderForm returnOrderForm, IPayment payment);
    }
}
