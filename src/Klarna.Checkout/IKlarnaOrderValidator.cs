using Klarna.Checkout.Models;

namespace Klarna.Checkout
{
    public interface IKlarnaOrderValidator
    {
        bool Compare(PatchedCheckoutOrderData checkoutData, PatchedCheckoutOrderData otherCheckoutOrderData);
    }
}