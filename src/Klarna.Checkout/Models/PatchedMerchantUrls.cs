using System;
using Klarna.Rest.Models;
using Newtonsoft.Json;

namespace Klarna.Checkout.Models
{
    public class PatchedMerchantUrls : MerchantUrls
    {
        [JsonProperty("cancellation_terms")]
        public Uri CancellationTerms { get; set; }
    }
}
