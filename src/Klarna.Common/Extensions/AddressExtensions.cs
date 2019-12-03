using System;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Klarna.Common.Helpers;
using Klarna.Rest.Core.Model;

namespace Klarna.Common.Extensions
{
    public static class AddressExtensions
    {
        private static Injected<IOrderGroupFactory> _orderGroupFactory;

        public static OrderManagementAddressInfo ToAddress(this IOrderAddress orderAddress)
        {
            var address = new OrderManagementAddressInfo();
            address.GivenName = orderAddress.FirstName;
            address.FamilyName = orderAddress.LastName;
            address.StreetAddress = orderAddress.Line1;
            address.StreetAddress2 = orderAddress.Line2;
            address.PostalCode = orderAddress.PostalCode;
            address.City = orderAddress.City;
            address.Country = CountryCodeHelper.GetTwoLetterCountryCode(orderAddress.CountryCode);
            if (orderAddress.CountryCode != null && address.Country.Equals("us", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(orderAddress.RegionName))
            {
                address.Region =
                    CountryCodeHelper.GetStateCode(CountryCodeHelper.GetTwoLetterCountryCode(orderAddress.CountryCode),
                        orderAddress.RegionName);
            }
            else
            {
                address.Region = orderAddress.RegionName;
            }

            address.Email = orderAddress.Email;
            address.Phone = orderAddress.DaytimePhoneNumber ?? orderAddress.EveningPhoneNumber;

            return address;
        }


        public static CheckoutAddressInfo ToCheckoutAddress(this IOrderAddress orderAddress)
        {
            var address = new CheckoutAddressInfo();
            address.GivenName = orderAddress.FirstName;
            address.FamilyName = orderAddress.LastName;
            address.StreetAddress = orderAddress.Line1;
            address.StreetAddress2 = orderAddress.Line2;
            address.PostalCode = orderAddress.PostalCode;
            address.City = orderAddress.City;
            address.Country = CountryCodeHelper.GetTwoLetterCountryCode(orderAddress.CountryCode);
            if (orderAddress.CountryCode != null && address.Country.Equals("us", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(orderAddress.RegionName))
            {
                address.Region =
                    CountryCodeHelper.GetStateCode(CountryCodeHelper.GetTwoLetterCountryCode(orderAddress.CountryCode),
                        orderAddress.RegionName);
            }
            else
            {
                address.Region = orderAddress.RegionName;
            }

            address.Email = orderAddress.Email;
            address.Phone = orderAddress.DaytimePhoneNumber ?? orderAddress.EveningPhoneNumber;

            return address;
        }


        public static IOrderAddress ToOrderAddress(this CheckoutAddressInfo address, ICart cart)
        {
            var addressId = $"{address.StreetAddress}{address.StreetAddress2}{address.City}";
            var orderAddress = cart.CreateOrderAddress(_orderGroupFactory.Service, addressId);

            orderAddress.FirstName = address.GivenName;
            orderAddress.LastName = address.FamilyName;
            orderAddress.Line1 = address.StreetAddress;
            orderAddress.Line2 = address.StreetAddress2;
            orderAddress.PostalCode = address.PostalCode;
            orderAddress.City = address.City;
            if (!string.IsNullOrEmpty(address.Country) && address.Country.Equals("us", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(address.Region))
            {
                orderAddress.RegionName = CountryCodeHelper.GetStateName(address.Country, address.Region);
                orderAddress.RegionCode = address.Region;
            }
            else
            {
                orderAddress.RegionName = orderAddress.RegionCode = address.Region;
            }
            orderAddress.CountryCode = CountryCodeHelper.GetThreeLetterCountryCode(address.Country);
            orderAddress.Email = address.Email;
            orderAddress.DaytimePhoneNumber = address.Phone;

            return orderAddress;
        }
    }
}