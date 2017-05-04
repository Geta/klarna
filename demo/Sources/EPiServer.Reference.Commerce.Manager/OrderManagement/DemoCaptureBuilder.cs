using EPiServer.Commerce.Order;
using Klarna.OrderManagement;
using Klarna.Rest.Models;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Manager.OrderManagement
{
    public class DemoCaptureBuilder : CaptureBuilder
    {
        public override CaptureData Build(CaptureData captureData, IOrderGroup orderGroup, IOrderForm returnOrderForm, IPayment payment)
        {
            return captureData;
        }
    }
}