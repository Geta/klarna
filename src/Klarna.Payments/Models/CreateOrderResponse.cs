using Klarna.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Klarna.Payments.Models
{
    public class CreateOrderResponse
    {
        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("redirect_url")]
        public string RedirectUrl { get; set; }

        [JsonProperty("fraud_status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FraudStatus FraudStatus { get; set; }
    }
}
