using Klarna.Checkout.Models;

namespace Klarna.Checkout
{
    public interface IKlarnaOrderValidator
    {
        bool Compare(CheckoutOrder checkoutData, CheckoutOrder otherCheckoutOrderData);
    }
}