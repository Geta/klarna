using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Klarna.Common;
using Klarna.OrderManagement.Models;
using Klarna.OrderManagement.Refunds;
using Klarna.Rest;
using Klarna.Rest.Models;
using Klarna.Rest.Models.Requests;
using Klarna.Rest.OrderManagement;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement
{
    public class KlarnaOrderService : IKlarnaOrderService
    {
        private readonly Client _client;
        private readonly IKlarnaOrderServiceApi _klarnaOrderServiceApi;

        internal KlarnaOrderService(Client client, IKlarnaOrderServiceApi klarnaOrderServiceApi)
        {
            _client = client;
            _klarnaOrderServiceApi = klarnaOrderServiceApi;
        }

        public void AcknowledgeOrder(IPurchaseOrder purchaseOrder)
        {
            _client.NewOrder(purchaseOrder.Properties[Constants.KlarnaOrderIdField]?.ToString()).Acknowledge();
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

        public CaptureData CaptureOrder(string orderId, int? amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment)
        {
            var order = _client.NewOrder(orderId);
            var capture = _client.NewCapture(order.Location);

            var lines = orderForm.GetAllLineItems().Select(l => FromLineItem(l, orderGroup.Currency)).ToList();

            var captureData = new CaptureData
            {
                CapturedAmount = amount,
                Description = description,
                OrderLines = lines
            };

            if (ServiceLocator.Current.TryGetExistingInstance(out ICaptureBuilder captureBuilder))
            {
                captureData = captureBuilder.Build(captureData, orderGroup, orderForm, payment);
            }
            capture.Create(captureData);
            return capture.Fetch();
        }

        public CaptureData CaptureOrder(string orderId, int? amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment, IShipment shipment)
        {
            var order = _client.NewOrder(orderId);
            var capture = _client.NewCapture(order.Location);

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

            if (ServiceLocator.Current.TryGetExistingInstance(out ICaptureBuilder captureBuilder))
            {
                captureData = captureBuilder.Build(captureData, orderGroup, orderForm, payment);
            }

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

            if (ServiceLocator.Current.TryGetExistingInstance(out IRefundBuilder refundBuilder))
            {
                refund = refundBuilder.Build(refund, orderGroup, orderForm, payment);
            }

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

        public Task<PatchedOrderData> GetOrder(string orderId)
        {
            return _klarnaOrderServiceApi.GetOrder(orderId);
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
