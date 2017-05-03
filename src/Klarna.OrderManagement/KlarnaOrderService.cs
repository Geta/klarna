using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Klarna.Common;
using Klarna.OrderManagement.Refunds;
using Klarna.Rest;
using Klarna.Rest.Models;
using Klarna.Rest.Models.Requests;
using Klarna.Rest.OrderManagement;
using Klarna.Rest.Transport;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement
{
    public class KlarnaOrderService : IKlarnaOrderService
    {
        private readonly Client _client;
        private Injected<RefundBuilder> _refundBuilder;
        private Injected<CaptureBuilder> _captureBuilder;

        public KlarnaOrderService(ConnectionConfiguration connectionConfiguration)
        {
            var connector = ConnectorFactory.Create(connectionConfiguration.Username, connectionConfiguration.Password, new Uri(connectionConfiguration.ApiUrl));

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

            var shipment =
                orderForm.Shipments.FirstOrDefault(x => x.GetShippingItemsTotal(orderGroup.Currency).Amount +
                                                        (x.GetShippingCost(orderGroup.Market, orderGroup.Currency).Amount - x.GetShipmentDiscountPrice(orderGroup.Currency).Amount) ==
                                                        payment.Amount);
            if (shipment == null)
            {
                throw new InvalidOperationException("Can't find correct shipment");
            }
            var lines = shipment.LineItems.Select(l => FromLineItem(l, orderGroup.Currency)).ToList();
            var shippingInfo = new ShippingInfo
            {
                // TODO shipping info
                ShippingMethod = "Own",
                TrackingNumber = shipment.ShipmentTrackingNumber
            };

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

        public void ReleaseRemaininAuthorization(string orderId)
        {
            IOrder order = _client.NewOrder(orderId);

            order.ReleaseRemainingAuthorization();
        }

        public void TriggerSendOut(string orderId, string captureId)
        {
            IOrder order = _client.NewOrder(orderId);
            ICapture capture = _client.NewCapture(order.Location, captureId);

            capture.TriggerSendOut();
        }

        public OrderData GetOrder(string orderId)
        {
            var order = _client.NewOrder(orderId);

            return order.Fetch();
        }

        public void ExtendAuthorizationTime(string orderId)
        {
            IOrder order = _client.NewOrder(orderId);

            order.ExtendAuthorizationTime();
        }

        public void UpdateCustomerInformation(string orderId, UpdateCustomerDetails updateCustomerDetails)
        {
            IOrder order = _client.NewOrder(orderId);

            order.UpdateCustomerDetails(updateCustomerDetails);
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
