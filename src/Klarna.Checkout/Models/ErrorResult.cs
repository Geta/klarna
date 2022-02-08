using System.Text.Json.Serialization;

namespace Klarna.Checkout.Models
{
    public class ErrorResult
    {
        [JsonPropertyName("error_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ErrorType ErrorType { get; set; }

        [JsonPropertyName("error_text")]
        public string ErrorText { get; set; }
    }
}
