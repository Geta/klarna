using EPiServer.Commerce.Order;
using Klarna.Checkout.Models;
using Klarna.Rest.Models;


namespace Klarna.Checkout
{
    public interface ICheckoutOrderDataBuilder
    {
        CheckoutOrderData Build(CheckoutOrderData checkoutOrderData, ICart cart, CheckoutConfiguration checkoutConfiguration);

        AddressUpdateResponse Build(AddressUpdateResponse addressUpdateResponse, ICart cart, CheckoutConfiguration checkoutConfiguration);

        ShippingOptionUpdateResponse Build(ShippingOptionUpdateResponse addressUpdateResponse, ICart cart, CheckoutConfiguration checkoutConfiguration);
    }
}
