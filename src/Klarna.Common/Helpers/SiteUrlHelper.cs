using EPiServer.Web;

namespace Klarna.Common.Helpers
{
    public static class SiteUrlHelper
    {
        public static string GetAbsoluteUrl()
        {
            var siteDefinition = SiteDefinition.Current;
            if (siteDefinition != null)
            {
                var siteUrl = siteDefinition.SiteUrl.ToString();
                
                return siteUrl.Substring(0, siteUrl.LastIndexOf("/") !=1 ? siteUrl.LastIndexOf("/") : (siteUrl.Length - 1));
            }
            return string.Empty;
        }
    }
}
