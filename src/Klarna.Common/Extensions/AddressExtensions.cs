using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Klarna.Common.Helpers;
using Klarna.Rest.Models;
using Mediachase.Commerce.Orders;

namespace Klarna.Common.Extensions
{
    public static class AddressExtensions
    {
        private static Injected<IOrderGroupFactory> _orderGroupFactory;

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
            address.Country = CountryCodeHelper.GetTwoLetterCountryCode(orderAddress.CountryCode);
            address.Email = orderAddress.Email;
            address.Phone = orderAddress.DaytimePhoneNumber ?? orderAddress.EveningPhoneNumber;

            return address;
        }

        public static IOrderAddress ToOrderAddress(this Address address, ICart cart)
        {
            var orderAddress = cart.CreateOrderAddress(_orderGroupFactory.Service);
            orderAddress.Id = $"{address.StreetAddress}{address.StreetAddress2}{address.City}";

            orderAddress.FirstName = address.GivenName;
            orderAddress.LastName = address.FamilyName;
            orderAddress.Line1 = address.StreetAddress;
            orderAddress.Line2 = address.StreetAddress2;
            orderAddress.PostalCode = address.PostalCode;
            orderAddress.City = address.City;
            orderAddress.RegionCode = address.Region;
            orderAddress.CountryCode = address.Country;
            orderAddress.Email = address.Email;
            orderAddress.DaytimePhoneNumber = address.Phone;

            return orderAddress;
        }
    }
}