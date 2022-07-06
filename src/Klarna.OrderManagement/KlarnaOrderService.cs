using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Klarna.Common;
using Klarna.Common.Configuration;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement
{
    public class KlarnaOrderService : KlarnaService, IKlarnaOrderService
    {
        private readonly OrderManagementStore _client;
        private readonly IOrderGroupCalculator _orderGroupCalculator;

        public KlarnaOrderService(OrderManagementStore client, IOrderRepository orderRepository, IPaymentProcessor paymentProcessor, IOrderGroupCalculator orderGroupCalculator, 
            IMarketService marketService, IConfigurationLoader configurationLoader) : base(orderRepository, paymentProcessor, orderGroupCalculator, marketService, configurationLoader)
        {
            _client = client;
            _orderGroupCalculator = orderGroupCalculator;
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
            var lines = GetOrderLines(orderGroup, _orderGroupCalculator.GetOrderGroupTotals(orderGroup), false);

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

            var lines = GetOrderLines(orderGroup, _orderGroupCalculator.GetOrderGroupTotals(orderGroup), false, shipment);

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

        public virtual async Task Refund(string orderId, IOrderGroup orderGroup, OrderForm orderForm, IPayment payment, IShipment shipment)
        {
            var lines = GetOrderLines(orderGroup, _orderGroupCalculator.GetOrderGroupTotals(orderGroup), false, shipment);

            var refund = new OrderManagementRefund
            {
                RefundedAmount = AmountHelper.GetAmount(payment.Amount),
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
    }
}
