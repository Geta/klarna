using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class NotificationModel
    {
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }
        [JsonPropertyName("event_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NotificationFraudStatus Status { get; set; }
    }
}
