using EPiServer.ServiceLocation;

namespace Klarna.Checkout
{
    [ServiceConfiguration(typeof(ICheckoutLanguageIdConverter))]
    public class DefaultCheckoutLanguageIdConverter : ICheckoutLanguageIdConverter
    {
        public string ConvertToCheckoutLanguageId(string languageId)
        {
            return languageId == "no" ? "nb" : languageId;
        }
    }
}
