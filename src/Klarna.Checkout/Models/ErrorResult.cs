using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Klarna.Checkout.Models
{
    public class ErrorResult
    {
        [JsonProperty("error_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ErrorType ErrorType { get; set; }

        [JsonProperty("error_text")]
        public string ErrorText { get; set; }
    }
}
