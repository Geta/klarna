using System;
using EPiServer.ServiceLocation;
using Klarna.Checkout.Models;

namespace Klarna.Checkout
{
    [ServiceConfiguration(typeof(IKlarnaOrderValidator))]
    public class KlarnaOrderValidator : IKlarnaOrderValidator
    {
        public bool Compare(PatchedCheckoutOrderData checkoutData, PatchedCheckoutOrderData otherCheckoutOrderData)
        {
            return checkoutData.OrderAmount.Equals(otherCheckoutOrderData.OrderAmount) &&
                   checkoutData.OrderTaxAmount.Equals(otherCheckoutOrderData.OrderTaxAmount) &&
                   checkoutData.ShippingCheckoutAddress != null &&
                   otherCheckoutOrderData.ShippingCheckoutAddress != null &&
                   checkoutData.ShippingCheckoutAddress.PostalCode.Equals(otherCheckoutOrderData.ShippingCheckoutAddress.PostalCode, StringComparison.InvariantCultureIgnoreCase) &&
                   (checkoutData.ShippingCheckoutAddress.Region == null && otherCheckoutOrderData.ShippingCheckoutAddress.Region == null ||
                   checkoutData.ShippingCheckoutAddress.Region.Equals(otherCheckoutOrderData.ShippingCheckoutAddress.Region, StringComparison.InvariantCultureIgnoreCase)) &&
                   checkoutData.ShippingCheckoutAddress.Country.Equals(otherCheckoutOrderData.ShippingCheckoutAddress.Country, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}