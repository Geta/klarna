using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Personalization.Providers.MaxMind;
using EPiServer.ServiceLocation;
using ISO3166;

namespace Klarna.Common.Helpers
{
    public static class CountryCodeHelper
    {
        private static Injected<GeolocationProvider> GeoLocationProvider { get; set; }

        public static string GetTwoLetterCountryCode(string threeLetterCode)
        {
            return ISO3166.Country.List.FirstOrDefault(x => x.ThreeLetterCode.Equals(threeLetterCode, StringComparison.InvariantCultureIgnoreCase))?.TwoLetterCode;
        }

        public static string GetThreeLetterCountryCode(string twoLetterCode)
        {
            return ISO3166.Country.List.FirstOrDefault(x => x.TwoLetterCode.Equals(twoLetterCode, StringComparison.InvariantCultureIgnoreCase))?.ThreeLetterCode;
        }

        public static IEnumerable<Country> GetCountryCodes()
        {
            return ISO3166.Country.List;
        }

        public static string GetContinentByCountry(string countryCode)
        {
            var continents = GeoLocationProvider.Service.GetContinentCodes();

            if (countryCode.Length == 3)
            {
                countryCode = GetTwoLetterCountryCode(countryCode);
            }

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
