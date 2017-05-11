using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Klarna.Checkout.Models;
using Klarna.Common;
using Klarna.Rest.Models;

namespace Klarna.Checkout
{
    public interface IKlarnaCheckoutService : IKlarnaService
    {
        CheckoutOrderData CreateOrUpdateOrder(ICart cart);
        CheckoutOrderData CreateOrder(ICart cart);
        CheckoutOrderData UpdateOrder(string orderId, ICart cart);

        CheckoutOrderData GetOrder(string orderId);

        ICart GetCartByKlarnaOrderId(int orderGroupId, string orderId);

        ShippingOptionUpdateResponse UpdateShippingMethod(ICart cart, ShippingOptionUpdateRequest shippingOptionUpdateRequest);

        AddressUpdateResponse UpdateAddress(ICart cart, AddressUpdateRequest addressUpdateRequest);
    }
}