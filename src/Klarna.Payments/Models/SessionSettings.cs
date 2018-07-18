using System;
using System.Collections.Generic;

namespace Klarna.Payments.Models
{
    public class SessionSettings
    {
        public SessionSettings(Uri siteUrl)
        {
            SiteUrl = siteUrl;
            AdditionalValues = new Dictionary<string, object>();
        }

        public IDictionary<string, object> AdditionalValues { get; set; }
        public Uri SiteUrl { get; }
    }
}