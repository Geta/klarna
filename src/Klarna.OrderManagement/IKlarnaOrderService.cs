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
        Task CancelOrder(string orderId);

        Task UpdateMerchantReferences(string orderId, string merchantReference1, string merchantReference2);
        Task<OrderManagementCapture> CaptureOrder(string orderId, int amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment);

        Task<OrderManagementCapture> CaptureOrder(string orderId, int amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment, IShipment shipment);

        Task Refund(string orderId, IOrderGroup orderGroup, OrderForm orderForm, IPayment payment);

        Task ReleaseRemainingAuthorization(string orderId);

        Task TriggerSendOut(string orderId, string captureId);

        Task<PatchedOrderData> GetOrder(string orderId);

        Task ExtendAuthorizationTime(string orderId);

        Task UpdateCustomerInformation(string orderId, OrderManagementCustomerAddresses updateCustomerDetails);
        Task AcknowledgeOrder(IPurchaseOrder purchaseOrder);
    }
}