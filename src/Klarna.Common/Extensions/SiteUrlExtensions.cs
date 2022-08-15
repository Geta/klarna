using System;
using Klarna.Common.Helpers;

namespace Klarna.Common.Extensions
{
    public static class SiteUrlExtensions
    {
        public static string ToAbsoluteUrl(this string url)
        {
            var siteUri = SiteUrlHelper.GetCurrentSiteUrl();
            if (siteUri == null || url == null) return null;
            return new Uri(siteUri, url).ToString();
        }
    }
}