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
                   checkoutData.ShippingAddress != null &&
                   otherCheckoutOrderData.ShippingAddress != null &&
                   checkoutData.ShippingAddress.PostalCode.Equals(otherCheckoutOrderData.ShippingAddress.PostalCode, StringComparison.InvariantCultureIgnoreCase) &&
                   (checkoutData.ShippingAddress.Region == null && otherCheckoutOrderData.ShippingAddress.Region == null ||
                   checkoutData.ShippingAddress.Region.Equals(otherCheckoutOrderData.ShippingAddress.Region, StringComparison.InvariantCultureIgnoreCase)) &&
                   checkoutData.ShippingAddress.Country.Equals(otherCheckoutOrderData.ShippingAddress.Country, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}