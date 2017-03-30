using Newtonsoft.Json;

namespace Klarna.Payments.Models
{
    public class CreateOrderResponse
    {
        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("redirect_url")]
        public string RedirectUrl { get; set; }

        [JsonProperty("fraud_status")]
        public string FraudStatus { get; set; }
    }
}
