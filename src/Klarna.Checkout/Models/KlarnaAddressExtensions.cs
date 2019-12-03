using System.Linq;
using Klarna.Rest.Core.Model;

namespace Klarna.Checkout.Models
{
    public static class KlarnaAddressExtensions
    {
        public static bool IsValid(this CheckoutAddressInfo address)
        {
            if (address == null)
            {
                return false;
            }

            var required = new[]
            {
                address.GivenName,
                address.FamilyName,
                address.Country,
                address.City,
                address.PostalCode,
                address.StreetAddress
            };
            return required.All(x => !string.IsNullOrEmpty(x));
        }
    }
}