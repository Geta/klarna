using EPiServer.Commerce.Order;
using Klarna.Checkout.Models;
using Klarna.Common.Configuration;

namespace Klarna.Checkout
{
    public interface ICheckoutOrderDataBuilder
    {
        CheckoutOrder Build(CheckoutOrder checkoutOrderData, ICart cart, CheckoutConfiguration checkoutConfiguration);

        AddressUpdateResponse Build(AddressUpdateResponse addressUpdateResponse, ICart cart, CheckoutConfiguration checkoutConfiguration);

        ShippingOptionUpdateResponse Build(ShippingOptionUpdateResponse addressUpdateResponse, ICart cart, CheckoutConfiguration checkoutConfiguration);
    }
}
