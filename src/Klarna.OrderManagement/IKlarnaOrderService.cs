using System.Collections.Generic;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Klarna.OrderManagement.Models;
using Klarna.Rest.Core.Model;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement
{
    public interface IKlarnaOrderService
    {
        void CancelOrder(string orderId);

        void UpdateMerchantReferences(string orderId, string merchantReference1, string merchantReference2);
        OrderManagementCapture CaptureOrder(string orderId, int amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment);

        OrderManagementCapture CaptureOrder(string orderId, int amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment, IShipment shipment);

        void Refund(string orderId, IOrderGroup orderGroup, OrderForm orderForm, IPayment payment);

        void ReleaseRemaininAuthorization(string orderId);

        void TriggerSendOut(string orderId, string captureId);

        Task<PatchedOrderData> GetOrder(string orderId);

        void ExtendAuthorizationTime(string orderId);

        void UpdateCustomerInformation(string orderId, OrderManagementCustomerAddresses updateCustomerDetails);
        void AcknowledgeOrder(IPurchaseOrder purchaseOrder);
    }
}