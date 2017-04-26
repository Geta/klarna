using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EPiServer.Commerce.Order;
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

        public CaptureData CaptureOrder(string orderId, int? amount, string description,
            ShippingInfo shippingInfo = null, List<OrderLine> orderLines = null)
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
                captureData.ShippingInfo = new List<ShippingInfo> {shippingInfo};
            }

            if (orderLines != null && orderLines.Any())
            {
                captureData.OrderLines = orderLines;
            }

            capture.Create(captureData);
            return capture.Fetch();

        }
        public void Refund(string orderId, IOrderForm orderForm)
        {
            IOrder order = _client.NewOrder(orderId);

            List<OrderLine> lines = new List<OrderLine>();

            lines.Add(new OrderLine()
            {
                Type = "physical",
                Reference = "123050",
                Name = "Tomatoes",
                Quantity = 5,
                QuantityUnit = "kg",
                UnitPrice = 600,
                TaxRate = 2500,
                TotalAmount = 3000,
                TotalTaxAmount = 600
            });

            Refund refund = new Refund()
            {
                RefundedAmount = 3000,
                Description = "Refunding half the tomatoes",
                OrderLines = lines
            };
            order.Refund(refund);
        }
    }
}
