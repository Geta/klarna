using System.Globalization;

namespace Klarna.Common
{
    public interface ILanguageService
    {
        string ConvertToLocale(string languageId);
        CultureInfo GetPreferredCulture();
    }
}
