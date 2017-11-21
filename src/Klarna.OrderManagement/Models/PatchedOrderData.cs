using Klarna.Payments.Models;
using Klarna.Rest.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Klarna.OrderManagement.Models
{
    public class PatchedOrderData : OrderData
    {
        [JsonProperty("fraud_status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FraudStatus FraudStatus { get; set; }
    }
}
