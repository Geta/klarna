using System.Linq;
using EPiServer.Commerce.Order;
using Klarna.Checkout;
using Klarna.Rest.Models;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout
{
    public class DemoCheckoutOrderDataBuilder : ICheckoutOrderDataBuilder
    {
        public CheckoutOrderData Build(CheckoutOrderData checkoutOrderData, ICart cart, Klarna.Checkout.Configuration configuration)
        {
            if (configuration.PrefillAddress)
            {
                // Try to parse address into dutch address lines
                if (checkoutOrderData.ShippingAddress.Country.Equals("NL"))
                {
                    var dutchAddress = ConvertToDutchAddress(checkoutOrderData.ShippingAddress);
                    checkoutOrderData.ShippingAddress = dutchAddress;
                }
            }
            return checkoutOrderData;
        }

        private Address ConvertToDutchAddress(Address address)
        {
            // Just an example, do not use

            var splitAddress = address.StreetAddress.Split(' ');
            address.StreetName = splitAddress.FirstOrDefault();
            address.StreetNumber = splitAddress.ElementAtOrDefault(1);
            
            address.StreetAddress = string.Empty;
            address.StreetAddress2 = string.Empty;

            return address;
        }
    }
}