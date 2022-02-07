using System.Text.Json.Serialization;

namespace Klarna.Payments.Models
{
    public class MerchantUrl
    {
        [JsonPropertyName("confirmation")]
        public string Confirmation { get; set; }

        [JsonPropertyName("notification")]
        public string Notification { get; set; }

        [JsonPropertyName("push")]
        public string Push { get; set; }
    }
}