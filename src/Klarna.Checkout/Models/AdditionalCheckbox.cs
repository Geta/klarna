using System.Text.Json.Serialization;

namespace Klarna.Checkout.Models
{
    public class AdditionalCheckbox
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("checked")]
        public bool? Checked { get; set; }

        [JsonPropertyName("required")]
        public bool? Required { get; set; }
    }
}
