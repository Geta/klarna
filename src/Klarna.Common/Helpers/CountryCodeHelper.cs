using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Personalization;
using EPiServer.ServiceLocation;
using ISO3166;

namespace Klarna.Common.Helpers
{
    public static class CountryCodeHelper
    {
        private static Injected<IGeolocationProvider> GeoLocationProvider { get; set; }
        private static Injected<ICountryRegionProvider> CountryRegionProvider { get; set; }

        public static string GetTwoLetterCountryCode(string countryCode)
        {
            return Country.List.FirstOrDefault(x => x.ThreeLetterCode.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase))?.TwoLetterCode
                   ?? Country.List.FirstOrDefault(x => x.TwoLetterCode.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase))?.TwoLetterCode;
        }

        public static string GetThreeLetterCountryCode(string countryCode)
        {
            return Country.List.FirstOrDefault(x => x.TwoLetterCode.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase))?.ThreeLetterCode
                   ?? Country.List.FirstOrDefault(x => x.ThreeLetterCode.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase))?.ThreeLetterCode;
        }

        public static IEnumerable<string> GetTwoLetterCountryCodes(IEnumerable<string> threeLetterCodes)
        {
            var newList = new List<string>();
            foreach (var item in threeLetterCodes)
            {
                newList.Add(GetTwoLetterCountryCode(item));
            }
            return newList;
        }

        public static string GetContinentByCountry(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                return string.Empty;
            }
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

        public static string GetStateName(string twoLetterCountryCode, string stateCode)
        {
            return CountryRegionProvider.Service.GetStateName(twoLetterCountryCode, stateCode);
        }
        public static string GetStateCode(string twoLetterCountryCode, string stateName)
        {
            return CountryRegionProvider.Service.GetStateCode(twoLetterCountryCode, stateName);
        }
    }
}
