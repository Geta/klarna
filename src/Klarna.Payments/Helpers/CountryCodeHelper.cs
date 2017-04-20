using System;
using System.Linq;
using EPiServer.Personalization.Providers.MaxMind;
using EPiServer.ServiceLocation;

namespace Klarna.Payments.Helpers
{
    public static class CountryCodeHelper
    {
        private static Injected<GeolocationProvider> GeoLocationProvider { get; set; }

        public static string GetTwoLetterCountryCode(string code)
        {
            return ISO3166.Country.List.FirstOrDefault(x => x.ThreeLetterCode.Equals(code, StringComparison.InvariantCultureIgnoreCase))?.TwoLetterCode;
        }

        public static string GetContinentByCountry(string countryCode)
        {
            var continents = GeoLocationProvider.Service.GetContinentCodes();

            foreach (var continent in continents)
            {
                if (GeoLocationProvider.Service.GetCountryCodes(continent).Any(x => x.Equals(countryCode)))
                {
                    return continent;
                }
            }
            return string.Empty;
        }
    }
}
