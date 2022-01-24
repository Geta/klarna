using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Klarna.Common.Models
{
    public class NotificationModel
    {
        [JsonProperty("order_id")]
        public string OrderId { get; set; }
        [JsonProperty("event_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationFraudStatus Status { get; set; }
    }
}
