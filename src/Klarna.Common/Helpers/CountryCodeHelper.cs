using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer.Personalization;
using EPiServer.ServiceLocation;

namespace Klarna.Common.Helpers
{
    /// <summary>
    /// TODO write tests for GetTwoLetterCountryCode and GetThreeLetterCountryCode
    /// </summary>
    public static class CountryCodeHelper
    {
        private static Injected<IGeolocationProvider> GeoLocationProvider { get; set; }
        private static Injected<ICountryRegionProvider> CountryRegionProvider { get; set; }

        public static string GetTwoLetterCountryCode(string countryCode)
        {
            return CultureInfo.GetCultures(CultureTypes.SpecificCultures).FirstOrDefault(x =>
                           x.ThreeLetterISOLanguageName.Equals(countryCode,
                               StringComparison.InvariantCultureIgnoreCase))
                       ?.TwoLetterISOLanguageName
                   ?? CultureInfo.GetCultures(CultureTypes.SpecificCultures).FirstOrDefault(x =>
                           x.TwoLetterISOLanguageName.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase))
                       ?.TwoLetterISOLanguageName;
        }

        public static string GetThreeLetterCountryCode(string countryCode)
        {
            return CultureInfo.GetCultures(CultureTypes.SpecificCultures).FirstOrDefault(x =>
                           x.ThreeLetterISOLanguageName.Equals(countryCode,
                               StringComparison.InvariantCultureIgnoreCase))
                       ?.ThreeLetterISOLanguageName
                   ?? CultureInfo.GetCultures(CultureTypes.SpecificCultures).FirstOrDefault(x =>
                           x.TwoLetterISOLanguageName.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase))
                       ?.ThreeLetterISOLanguageName;
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
