using System.Linq;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Extensions
{
    public static class ContentLoaderExtensions
    {
        private static Injected<ISiteDefinitionRepository> _siteDefinitionRepository = default(Injected<ISiteDefinitionRepository>);

        public static T GetFirstChild<T>(this IContentLoader contentLoader, ContentReference contentReference) where T : IContentData
        {
            return contentLoader.GetChildren<T>(contentReference).FirstOrDefault();
        }

        public static StartPage GetStartPage(this IContentLoader contentLoader)
        {
            var siteDefinitions = _siteDefinitionRepository.Service.List().ToList();

            return contentLoader.Get<StartPage>(siteDefinitions.First().StartPage);
        }
    }
}