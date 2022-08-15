using System.Text.Json.Serialization;

namespace Klarna.Payments.Models
{
    public class AssetUrls
    {
        [JsonPropertyName("descriptive")]
        public string Descriptive { get; set; }

        [JsonPropertyName("standard")]
        public string Standard { get; set; }
    }
}