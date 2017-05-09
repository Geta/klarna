using System;
using System.Linq;
using Klarna.Rest.Models;
using Mediachase.Commerce.Customers;

namespace Klarna.Payments.Extensions
{
    public static class CustomerAddressExtensions
    {
        public static Address ToAddress(this CustomerAddress customerAddress)
        {
            var address = new Address();
            address.GivenName = customerAddress.FirstName;
            address.FamilyName = customerAddress.LastName;
            address.StreetAddress = customerAddress.Line1;
            address.StreetAddress2 = customerAddress.Line2;
            address.PostalCode = customerAddress.PostalCode;
            address.City = customerAddress.City;
            address.Region = customerAddress.RegionCode;
            address.Country = GetTwoLetterCountryCode(customerAddress.CountryCode);
            address.Email = customerAddress.Email;
            address.Phone = customerAddress.DaytimePhoneNumber ?? customerAddress.EveningPhoneNumber;

            return address;
        }

        private static string GetTwoLetterCountryCode(string code)
        {
            return ISO3166.Country.List.FirstOrDefault(x => x.ThreeLetterCode.Equals(code, StringComparison.InvariantCultureIgnoreCase))?.TwoLetterCode;
        }
    }
}
