using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Klarna.Common.Models
{
    public class NotificationModel
    {
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }
        [JsonPropertyName("event_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationFraudStatus Status { get; set; }
    }
}
