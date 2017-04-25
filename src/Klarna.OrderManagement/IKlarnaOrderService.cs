using System;
using System.Collections.Generic;
using Klarna.Rest.Models;

namespace Klarna.OrderManagement
{
    public interface IKlarnaOrderService
    {
        void CancelOrder(string orderId);

        void UpdateMerchantReferences(string orderId, string merchantReference1, string merchantReference2);
        CaptureData CaptureOrder(string orderId, int? amount, string description, ShippingInfo shippingInfo = null, List<OrderLine> orderLines = null);
    }
}