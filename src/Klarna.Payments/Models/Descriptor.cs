using System.Text.Json.Serialization;

namespace Klarna.Payments.Models
{
    public class Descriptor
    {
        public Descriptor()
        {
            AssetUrls = new AssetUrls();
        }

        [JsonPropertyName("tagline")]
        public string Tagline { get; set; }

        [JsonPropertyName("asset_urls")]
        public AssetUrls AssetUrls { get; set; }
    }
}