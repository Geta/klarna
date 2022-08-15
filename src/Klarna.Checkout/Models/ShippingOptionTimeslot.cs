using System.Text.Json.Serialization;

namespace Klarna.Checkout.Models
{
    public class ShippingOptionTimeslot
    {
        /// <summary>
        /// Id.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// Start time.
        /// </summary>
        [JsonPropertyName("start")]
        public string Start { get; set; }
        
        /// <summary>
        /// End time.
        /// </summary>
        [JsonPropertyName("end")]
        public string End { get; set; }
    }
}