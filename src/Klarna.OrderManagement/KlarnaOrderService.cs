using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Klarna.OrderManagement.Refunds;
using Klarna.Rest;
using Klarna.Rest.Models;
using Klarna.Rest.Models.Requests;
using Klarna.Rest.OrderManagement;
using Klarna.Rest.Transport;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;

namespace Klarna.OrderManagement
{
    public class KlarnaOrderService : IKlarnaOrderService
    {
        private readonly Client _client;
        private Injected<RefundBuilder> _refundBuilder;
        private Injected<CaptureBuilder> _captureBuilder;

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

        public CaptureData CaptureOrder(
            string orderId,
            int? amount,
            string description,
            IOrderGroup orderGroup,
            IOrderForm orderForm,
            IPayment payment)
        {
            var order = _client.NewOrder(orderId);
            var capture = _client.NewCapture(order.Location);
            
            var shipment = orderForm.Shipments.FirstOrDefault(x => (x.GetShippingItemsTotal(orderGroup.Currency).Amount + x.GetShippingCost(orderGroup.Market, orderGroup.Currency).Amount) == payment.Amount);
            if (shipment == null)
            {
                return null;
            }
            var lines = shipment.LineItems.Select(l => FromLineItem(l, orderGroup.Currency)).ToList();
            var shippingInfo = new ShippingInfo
            {
                // TODO shipping info
                ShippingMethod = shipment.ShippingMethodName,
                TrackingNumber = shipment.ShipmentTrackingNumber
            };
            if (string.IsNullOrEmpty(shippingInfo.ShippingMethod))
            {
                var shipmentMethod = ShippingManager.GetShippingMethod(shipment.ShippingMethodId).ShippingMethod.FirstOrDefault();
                if (shipmentMethod != null)
                {
                    shippingInfo.ShippingMethod = shipmentMethod.DisplayName;
                }
            }

            var captureData = new CaptureData
            {
                CapturedAmount = amount,
                Description = description,
                OrderLines = lines,
                ShippingInfo = new List<ShippingInfo>() { shippingInfo }
            };
            captureData = _captureBuilder.Service.Build(captureData, orderGroup, orderForm, payment);

            capture.Create(captureData);
            return capture.Fetch();
        }

        public void Refund(string orderId, IOrderGroup orderGroup, OrderForm orderForm, IPayment payment)
        {
            IOrder order = _client.NewOrder(orderId);

            List<OrderLine> lines = orderForm.LineItems.Select(l => FromLineItem(l, orderGroup.Currency)).ToList();

            Refund refund = new Refund()
            {
                RefundedAmount = GetAmount(payment.Amount),
                Description = orderForm.ReturnComment,
                OrderLines = lines
            };

            refund = _refundBuilder.Service.Build(refund, orderGroup, orderForm, payment);

            order.Refund(refund);
        }

        private int GetAmount(decimal money)
        {
            if (money > 0)
            {
                return (int)(money * 100);
            }
            return 0;
        }

        private OrderLine FromLineItem(ILineItem item, Currency currency)
        {
            var orderLine = new OrderLine
            {
                Type = "physical",
                Reference = item.Code,
                Name = item.DisplayName,
                Quantity = (int)item.ReturnQuantity,
                UnitPrice = GetAmount(item.PlacedPrice),
                TotalAmount = GetAmount(item.GetExtendedPrice(currency).Amount),
                TotalDiscountAmount = GetAmount(item.GetEntryDiscount())
            };
            return orderLine;
        }
    }
}
