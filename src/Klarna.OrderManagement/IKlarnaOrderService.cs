using System.Collections.Generic;
using EPiServer.Commerce.Order;
using Klarna.Rest.Models;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement
{
    public interface IKlarnaOrderService
    {
        void CancelOrder(string orderId);

        void UpdateMerchantReferences(string orderId, string merchantReference1, string merchantReference2);
        CaptureData CaptureOrder(string orderId,
            int? amount,
            string description,
            IOrderGroup orderGroup,
            IOrderForm orderForm,
            IPayment payment);

        void Refund(string orderId, IOrderGroup orderGroup, OrderForm orderForm, IPayment payment);

        void ReleaseRemaininAuthorization(string orderId);

        void TriggerSendOutExample(string orderId, string captureId);

        OrderData GetOrder(string orderId);
    }
}