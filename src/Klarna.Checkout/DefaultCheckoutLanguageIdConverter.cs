using EPiServer.ServiceLocation;

namespace Klarna.Checkout
{
    /// <summary>
    /// Supported languages and locales: https://developers.klarna.com/documentation/klarna-checkout/in-depth/available-languages/
    /// </summary>
    [ServiceConfiguration(typeof(ICheckoutLanguageIdConverter))]
    public class DefaultCheckoutLanguageIdConverter : ICheckoutLanguageIdConverter
    {
        public string ConvertToCheckoutLanguageId(string languageId)
        {
            if (languageId.StartsWith("de"))
            {
                return "de";
            }

            if (languageId.StartsWith("nl"))
            {
                return "nl";
            }

            if (languageId.StartsWith("no"))
            {
                return "nb";
            }

            if (languageId.StartsWith("pt"))
            {
                return "pt";
            }

            return languageId;
        }
    }
}
