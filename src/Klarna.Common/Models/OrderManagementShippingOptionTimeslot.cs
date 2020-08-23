using Newtonsoft.Json;

namespace Klarna.Common.Models
{
    public class OrderManagementShippingOptionTimeslot
    {
        /// <summary>
        /// The timeslot id.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        
        /// <summary>
        /// Start of the timeslot.
        /// </summary>
        [JsonProperty(PropertyName = "start")]
        public string Start { get; set; }
        
        /// <summary>
        /// End of the timeslot.
        /// </summary>
        [JsonProperty(PropertyName = "end")]
        public string End { get; set; }
        
        /// <summary>
        /// Cutoff time for delivery.
        /// </summary>
        [JsonProperty(PropertyName = "cutoff")]
        public string Cutoff { get; set; }
        
        /// <summary>
        /// Price.
        /// </summary>
        [JsonProperty(PropertyName = "price")]
        public int Price { get; set; }
    }
}