namespace Klarna.Checkout
{
    public interface ICheckoutLanguageIdConverter
    {
        string ConvertToCheckoutLanguageId(string languageId);
    }
}
