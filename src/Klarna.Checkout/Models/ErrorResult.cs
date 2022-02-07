using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Klarna.Checkout.Models
{
    public class ErrorResult
    {
        [JsonPropertyName("error_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ErrorType ErrorType { get; set; }

        [JsonPropertyName("error_text")]
        public string ErrorText { get; set; }
    }
}
