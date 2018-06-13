using Newtonsoft.Json;

namespace Klarna.Payments.Models
{
    public class PaymentMethodCategory
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("asset_urls")]
        public AssetUrls AssetUrls { get; set; }
    }

    public class AssetUrls
    {
        [JsonProperty("descriptive")]
        public string Descriptive { get; set; }
        [JsonProperty("standard")]
        public string Standard { get; set; }
    }
}