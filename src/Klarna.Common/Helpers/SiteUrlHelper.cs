using System.Linq;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace Klarna.Common.Helpers
{
    public static class SiteUrlHelper
    {
        private static Injected<ISiteDefinitionRepository> SiteDefinitionRepository { get; set; }

        public static string GetAbsoluteUrl()
        {
            var siteDefinition = SiteDefinition.Current;
            if (siteDefinition != null && siteDefinition.SiteUrl != null)
            {
                return GetUrl(siteDefinition.SiteUrl.ToString());
            }
            else
            {
                var firstSite = SiteDefinitionRepository.Service.List().FirstOrDefault();
                if (firstSite != null && firstSite.SiteUrl != null)
                {
                    return GetUrl(firstSite.SiteUrl.ToString());
                }
            }
            return string.Empty;
        }

        private static string GetUrl(string siteUrl)
        {
            if (string.IsNullOrEmpty(siteUrl))
            {
                return string.Empty;
            }
            return siteUrl.Substring(0, siteUrl.LastIndexOf("/") != 1 ? siteUrl.LastIndexOf("/") : (siteUrl.Length - 1));
        }
    }
}
