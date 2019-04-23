using System;
using System.Linq;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace Klarna.Common.Helpers
{
    public static class SiteUrlHelper
    {
        private static Injected<ISiteDefinitionRepository> SiteDefinitionRepository { get; set; }

        private static SiteDefinition Site => SiteDefinition.Current;

        /// <summary>
        /// Returns primary host (non-wildcard) URL, fallback to site URL.
        /// </summary>
        public static Uri GetCurrentSiteUrl()
        {
            var site = GetCurrentSite();
            var primaryHost = Site
                .Hosts
                .FirstOrDefault(x => x.Type == HostDefinitionType.Primary && !x.IsWildcardHost());

            return primaryHost?.Url ?? site?.SiteUrl;
        }

        public static string GetAbsoluteUrl()
        {
            var siteUrl = GetCurrentSiteUrl();
            return siteUrl?.ToString().TrimEnd('/');
        }

        private static SiteDefinition GetCurrentSite()
        {
            return Site ?? SiteDefinitionRepository.Service.List().FirstOrDefault();
        }
    }
}
