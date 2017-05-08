using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Klarna.Rest.Models;

namespace Klarna.Checkout
{
    public interface IKlarnaCheckoutService
    {
        CheckoutOrderData CreateOrUpdateOrder(ICart cart);
        CheckoutOrderData CreateOrder(ICart cart);
        CheckoutOrderData UpdateOrder(string orderId, ICart cart);

        ICart GetCartByKlarnaOrderId(string orderId);
    }
}