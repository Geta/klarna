using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Klarna.Rest;
using Klarna.Rest.Models;
using Klarna.Rest.Models.Requests;
using Klarna.Rest.OrderManagement;
using Klarna.Rest.Transport;

namespace Klarna.OrderManagement
{
    public class KlarnaOrderService : IKlarnaOrderService
    {
        private readonly Client _client;

        public KlarnaOrderService(string merchantId, string sharedSecret, string apiUrl)
        {
            var connector = ConnectorFactory.Create(merchantId, sharedSecret, new Uri(apiUrl));

            _client = new Client(connector);
        }

        public void CancelOrder(string orderId)
        {
            var order = _client.NewOrder(orderId);
            order.Cancel();
        }

        public void UpdateMerchantReferences(string orderId, string merchantReference1, string merchantReference2)
        {
            var order = _client.NewOrder(orderId);

            var updateMerchantReferences = new UpdateMerchantReferences
            {
                MerchantReference1 = merchantReference1,
                MerchantReference2 = merchantReference2
            };
            order.UpdateMerchantReferences(updateMerchantReferences);
        }

        public CaptureData CaptureOrder(string orderId, int? amount, string description, ShippingInfo shippingInfo = null, List<OrderLine> orderLines = null)
        {
            var order = _client.NewOrder(orderId);
            var capture = _client.NewCapture(order.Location);

            var captureData = new CaptureData
            {
                CapturedAmount = amount,
                Description = description
            };

            if (shippingInfo != null)
            {
                captureData.ShippingInfo = new List<ShippingInfo> { shippingInfo };
            }

            if (orderLines != null && orderLines.Any())
            {
                captureData.OrderLines = orderLines;
            }

            capture.Create(captureData);
            return capture.Fetch();
        }
    }
}
