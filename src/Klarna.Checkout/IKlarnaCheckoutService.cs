using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Klarna.Checkout.Models;
using Klarna.Common;
using Klarna.Common.Configuration;
using Mediachase.Commerce;

namespace Klarna.Checkout
{
    public interface IKlarnaCheckoutService : IKlarnaService
    {
        Task<CheckoutOrder> CreateOrUpdateOrder(ICart cart);
        Task<CheckoutOrder> CreateOrder(ICart cart);
        Task<CheckoutOrder> UpdateOrder(string orderId, ICart cart);

        Task<CheckoutOrder> GetOrder(string klarnaOrderId, IMarket market);

        ICart GetCartByKlarnaOrderId(int orderGroupId, string orderId);

        ShippingOptionUpdateResponse UpdateShippingMethod(ICart cart, ShippingOptionUpdateRequest shippingOptionUpdateRequest);

        AddressUpdateResponse UpdateAddress(ICart cart, CallbackAddressUpdateRequest addressUpdateRequest);

        void CancelOrder(ICart cart);

        bool ValidateOrder(ICart cart, CheckoutOrder checkoutData);

        Task UpdateMerchantReference1(IPurchaseOrder purchaseOrder);
        void AcknowledgeOrder(IPurchaseOrder purchaseOrder);
        CheckoutConfiguration GetConfiguration(IMarket market);
        CheckoutConfiguration GetConfiguration(MarketId marketId);

        void Complete(IPurchaseOrder purchaseOrder);
    }
}