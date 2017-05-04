using EPiServer.Commerce.Order;
using Klarna.OrderManagement;
using Klarna.Rest.Models;

namespace EPiServer.Reference.Commerce.Manager.OrderManagement
{
    public class DemoCaptureBuilder : ICaptureBuilder
    {
        public CaptureData Build(CaptureData captureData, IOrderGroup orderGroup, IOrderForm returnOrderForm, IPayment payment)
        {
            // Here you can make changes to captureData if needed
            return captureData;
        }
    }
}