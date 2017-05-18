using EPiServer.Commerce.Order;
using Klarna.Rest.Models;


namespace Klarna.Checkout
{
    public interface ICheckoutOrderDataBuilder
    {
        CheckoutOrderData Build(CheckoutOrderData session, ICart cart);
    }
}
