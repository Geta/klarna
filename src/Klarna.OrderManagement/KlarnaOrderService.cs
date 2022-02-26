using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Klarna.Common;
using Klarna.Common.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement
{
    public class KlarnaOrderService : IKlarnaOrderService
    {
        private readonly OrderManagementStore _client;

        internal KlarnaOrderService(OrderManagementStore client)
        {
            _client = client;
        }

        public virtual async Task AcknowledgeOrder(IPurchaseOrder purchaseOrder)
        {
            await _client.AcknowledgeOrder(purchaseOrder.Properties[Constants.KlarnaOrderIdField]?.ToString()).ConfigureAwait(false);
        }

        public virtual async Task CancelOrder(string orderId)
        {
            await _client.CancelOrder(orderId).ConfigureAwait(false);
        }

        public virtual async Task UpdateMerchantReferences(string orderId, string merchantReference1, string merchantReference2)
        {
            var updateMerchantReferences = new OrderManagementMerchantReferences
            {
                MerchantReference1 = merchantReference1,
                MerchantReference2 = merchantReference2
            };

            await _client.UpdateMerchantReferences(orderId, updateMerchantReferences).ConfigureAwait(false);
        }

        public virtual async Task<OrderManagementCapture> CaptureOrder(string orderId, int amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment)
        {
            var lines = orderForm.GetAllLineItems().Select(l => FromLineItem(l, orderGroup.Currency)).ToList();

            var captureData = new OrderManagementCreateCapture
            {
                CapturedAmount = amount,
                Description = description,
                OrderLines = lines
            };
            return  await _client.CreateAndFetchCapture(orderId, captureData).ConfigureAwait(false);
        }

        public virtual async Task<OrderManagementCapture> CaptureOrder(string orderId, int amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment, IShipment shipment)
        {
            if (shipment == null)
            {
                throw new InvalidOperationException("Can't find correct shipment");
            }

            var lines = shipment.LineItems.Select(l => FromLineItem(l, orderGroup.Currency)).ToList();
            var shippingInfo = new OrderManagementShippingInfo
            {
                ShippingCompany = shipment.ShippingMethodName,
                ShippingMethod = OrderManagementShippingMethod.Own,
                TrackingNumber = shipment.ShipmentTrackingNumber
            };

            var captureData = new OrderManagementCreateCapture
            {
                CapturedAmount = amount,
                Description = description,
                OrderLines = lines,
                ShippingInfo = new List<OrderManagementShippingInfo>() { shippingInfo }
            };

            return await _client.CreateAndFetchCapture(orderId, captureData).ConfigureAwait(false);
        }

        public virtual async Task Refund(string orderId, IOrderGroup orderGroup, OrderForm orderForm, IPayment payment)
        {
            var lines = orderForm.LineItems.Select(l => FromLineItem(l, orderGroup.Currency)).ToList();

            var refund = new OrderManagementRefund
            {
                RefundedAmount = GetAmount(payment.Amount),
                Description = orderForm.ReturnComment,
                OrderLines = lines
            };

            await _client.CreateAndFetchRefund(orderId, refund).ConfigureAwait(false);
        }

        public virtual async Task ReleaseRemainingAuthorization(string orderId)
        { 
            await _client.ReleaseRemainingAuthorization(orderId).ConfigureAwait(false);
        }

        public virtual async Task TriggerSendOut(string orderId, string captureId)
        {
           await _client.TriggerResendOfCustomerCommunication(orderId, captureId).ConfigureAwait(false);
        }

        public virtual async Task<OrderManagementOrder> GetOrder(string orderId)
        {
            return await _client.GetOrder(orderId).ConfigureAwait(false);
        }

        public virtual async Task ExtendAuthorizationTime(string orderId)
        {
             await _client.ExtendAuthorizationTime(orderId).ConfigureAwait(false);
        }

        public virtual async Task UpdateCustomerInformation(string orderId, OrderManagementCustomerAddresses updateCustomerDetails)
        {
            await _client.UpdateCustomerAddresses(orderId, updateCustomerDetails).ConfigureAwait(false);
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
                Type = OrderLineType.physical,
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
