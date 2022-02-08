using System.Text.Json.Serialization;
using Klarna.Common.Models;

namespace Klarna.Payments.Models
{
    public class CreateOrderResponse
    {
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }

        [JsonPropertyName("redirect_url")]
        public string RedirectUrl { get; set; }

        [JsonPropertyName("fraud_status")]
        [Newtonsoft.Json.JsonConverter(typeof(JsonStringEnumConverter))]
        public FraudStatus FraudStatus { get; set; }
    }
}
