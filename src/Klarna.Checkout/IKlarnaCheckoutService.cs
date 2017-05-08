using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Klarna.Rest.Models;

namespace Klarna.Checkout
{
    public interface IKlarnaCheckoutService
    {
        CheckoutOrderData CreateOrUpdateSession(ICart cart);
        CheckoutOrderData GetOrder(ICart cart);
        CheckoutOrderData CreateOrder(ICart cart);
        CheckoutOrderData UpdateOrder(ICart cart);
    }
}