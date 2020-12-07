using System.Globalization;
using System.Linq;
using System.Threading;
using EPiServer.DataAbstraction;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;

namespace Klarna.Common
{
    [ServiceConfiguration(typeof(ILanguageService))]
    public class DefaultLanguageService : ILanguageService
    {
        private readonly ILanguageBranchRepository _languageBranchRepository;

        public DefaultLanguageService(ILanguageBranchRepository languageBranchRepository)
        {
            _languageBranchRepository = languageBranchRepository;
        }

        public CultureInfo GetPreferredCulture()
        {
            var languages = _languageBranchRepository.ListEnabled();
            var currentLanguage = ContentLanguage.PreferredCulture;

            if (languages.Any(x => x.Name == Thread.CurrentThread.CurrentCulture.Name))
            {
                currentLanguage = Thread.CurrentThread.CurrentCulture;
            }
            else if (languages.Any(x => x.Name == Thread.CurrentThread.CurrentCulture.Parent.Name))
            {
                currentLanguage = Thread.CurrentThread.CurrentCulture.Parent;
            }

            return currentLanguage;
        }

        /// <summary>
        /// Supported languages and locales: https://developers.klarna.com/documentation/klarna-checkout/in-depth/available-languages/
        /// </summary>
        public string ConvertToLocale(string languageId)
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
