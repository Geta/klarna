using System.Collections.Generic;
using EPiServer.Commerce.Order;
using Klarna.Rest.Models;
using Klarna.Rest.Models.Requests;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement
{
    public interface IKlarnaOrderService
    {
        void CancelOrder(string orderId);

        void UpdateMerchantReferences(string orderId, string merchantReference1, string merchantReference2);
        CaptureData CaptureOrder(string orderId, int? amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment, IShipment shipment);

        void Refund(string orderId, IOrderGroup orderGroup, OrderForm orderForm, IPayment payment);

        void ReleaseRemaininAuthorization(string orderId);

        void TriggerSendOut(string orderId, string captureId);

        OrderData GetOrder(string orderId);

        void ExtendAuthorizationTime(string orderId);

        void UpdateCustomerInformation(string orderId, UpdateCustomerDetails updateCustomerDetails);
        void AcknowledgeOrder(IPurchaseOrder purchaseOrder);
    }
}