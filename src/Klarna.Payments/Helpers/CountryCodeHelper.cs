using System;
using System.Linq;

namespace Klarna.Payments.Helpers
{
    public static class CountryCodeHelper
    {
        public static string GetTwoLetterCountryCode(string code)
        {
            return ISO3166.Country.List.FirstOrDefault(x => x.ThreeLetterCode.Equals(code, StringComparison.InvariantCultureIgnoreCase))?.TwoLetterCode;
        }
    }
}
