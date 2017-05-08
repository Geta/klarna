using System;
using System.Linq;
using EPiServer.Commerce.Order;
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

        public static Address ToAddress(this IOrderAddress orderAddress)
        {
            var address = new Address();
            address.GivenName = orderAddress.FirstName;
            address.FamilyName = orderAddress.LastName;
            address.StreetAddress = orderAddress.Line1;
            address.StreetAddress2 = orderAddress.Line2;
            address.PostalCode = orderAddress.PostalCode;
            address.City = orderAddress.City;
            address.Region = orderAddress.RegionCode;
            address.Country = GetTwoLetterCountryCode(orderAddress.CountryCode);
            address.Email = orderAddress.Email;
            address.Phone = orderAddress.DaytimePhoneNumber ?? orderAddress.EveningPhoneNumber;

            return address;
        }

        private static string GetTwoLetterCountryCode(string code)
        {
            return ISO3166.Country.List.FirstOrDefault(x => x.ThreeLetterCode.Equals(code, StringComparison.InvariantCultureIgnoreCase))?.TwoLetterCode;
        }
    }
}
