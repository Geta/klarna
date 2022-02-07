using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class OrderManagementShippingOptionTimeslot
    {
        /// <summary>
        /// The timeslot id.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// Start of the timeslot.
        /// </summary>
        [JsonPropertyName("start")]
        public string Start { get; set; }
        
        /// <summary>
        /// End of the timeslot.
        /// </summary>
        [JsonPropertyName("end")]
        public string End { get; set; }
        
        /// <summary>
        /// Cutoff time for delivery.
        /// </summary>
        [JsonPropertyName("cutoff")]
        public string Cutoff { get; set; }
        
        /// <summary>
        /// Price.
        /// </summary>
        [JsonPropertyName("price")]
        public int Price { get; set; }
    }
}