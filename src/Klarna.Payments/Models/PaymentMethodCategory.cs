using System.Text.Json.Serialization;

namespace Klarna.Payments.Models
{
    public class PaymentMethodCategory
    {
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("asset_urls")]
        public AssetUrls AssetUrls { get; set; }
    }

    public class AssetUrls
    {
        [JsonPropertyName("descriptive")]
        public string Descriptive { get; set; }

        [JsonPropertyName("standard")]
        public string Standard { get; set; }
    }
}