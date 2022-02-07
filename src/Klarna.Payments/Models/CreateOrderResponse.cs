using System.Text.Json.Serialization;
using Klarna.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Klarna.Payments.Models
{
    public class CreateOrderResponse
    {
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }

        [JsonPropertyName("redirect_url")]
        public string RedirectUrl { get; set; }

        [JsonPropertyName("fraud_status")]
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public FraudStatus FraudStatus { get; set; }
    }
}
