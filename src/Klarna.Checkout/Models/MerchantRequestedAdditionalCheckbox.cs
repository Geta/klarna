using System.Text.Json.Serialization;

namespace Klarna.Checkout.Models
{
    public class MerchantRequestedAdditionalCheckbox
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("checked")]
        public bool Checked { get; set; }
    }
}
