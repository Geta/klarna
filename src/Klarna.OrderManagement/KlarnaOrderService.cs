using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Klarna.Common;
using Klarna.OrderManagement.Models;
using Klarna.Rest.Core.Model;
using Klarna.Rest.Core.Model.Enum;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Client = Klarna.Rest.Core.Klarna;

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

        public async Task AcknowledgeOrder(IPurchaseOrder purchaseOrder)
        {
            await _client.OrderManagement.AcknowledgeOrder(purchaseOrder.Properties[Constants.KlarnaOrderIdField]?.ToString()).ConfigureAwait(false);
        }

        public async Task CancelOrder(string orderId)
        {
            await _client.OrderManagement.CancelOrder(orderId).ConfigureAwait(false);
        }

        public async Task UpdateMerchantReferences(string orderId, string merchantReference1, string merchantReference2)
        {
            var updateMerchantReferences = new OrderManagementMerchantReferences
            {
                MerchantReference1 = merchantReference1,
                MerchantReference2 = merchantReference2
            };

            await _client.OrderManagement.UpdateMerchantReferences(orderId, updateMerchantReferences).ConfigureAwait(false);
        }

        public async Task<OrderManagementCapture> CaptureOrder(string orderId, int amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment)
        {
            var lines = orderForm.GetAllLineItems().Select(l => FromLineItem(l, orderGroup.Currency)).ToList();

            var captureData = new OrderManagementCreateCapture
            {
                CapturedAmount = amount,
                Description = description,
                OrderLines = lines
            };
            return  await _client.OrderManagement.CreateAndFetchCapture(orderId, captureData).ConfigureAwait(false);
        }

        public async Task<OrderManagementCapture> CaptureOrder(string orderId, int amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment, IShipment shipment)
        {
            if (shipment == null)
            {
                throw new InvalidOperationException("Can't find correct shipment");
            }
            var lines = shipment.LineItems.Select(l => FromLineItem(l, orderGroup.Currency)).ToList();
            var shippingInfo = new OrderManagementShippingInfo
            {
                // TODO shipping info
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

            return await _client.OrderManagement.CreateAndFetchCapture(orderId, captureData).ConfigureAwait(false);
        }

        public async Task Refund(string orderId, IOrderGroup orderGroup, OrderForm orderForm, IPayment payment)
        {
            var lines = orderForm.LineItems.Select(l => FromLineItem(l, orderGroup.Currency)).ToList();

            var refund = new OrderManagementRefund
            {
                RefundedAmount = GetAmount(payment.Amount),
                Description = orderForm.ReturnComment,
                OrderLines = lines
            };
            await _client.OrderManagement.CreateAndFetchRefund(orderId, refund).ConfigureAwait(false);
        }

        public async Task ReleaseRemainingAuthorization(string orderId)
        { 
            await _client.OrderManagement.ReleaseRemainingAuthorization(orderId).ConfigureAwait(false);
        }

        public async Task TriggerSendOut(string orderId, string captureId)
        {
           await _client.OrderManagement.TriggerResendOfCustomerCommunication(orderId, captureId).ConfigureAwait(false);
        }

        public async Task<PatchedOrderData> GetOrder(string orderId)
        {
            return await _klarnaOrderServiceApi.GetOrder(orderId).ConfigureAwait(false);
        }

        public async Task ExtendAuthorizationTime(string orderId)
        {
             await _client.OrderManagement.ExtendAuthorizationTime(orderId).ConfigureAwait(false);
        }

        public async Task UpdateCustomerInformation(string orderId, OrderManagementCustomerAddresses updateCustomerDetails)
        {
            await _client.OrderManagement.UpdateCustomerAddresses(orderId, updateCustomerDetails).ConfigureAwait(false);
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
