using Newtonsoft.Json;

namespace Klarna.Common.Models
{
    public class ShippingOptionTimeslot
    {
        /// <summary>
        /// Id.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        
        /// <summary>
        /// Start time.
        /// </summary>
        [JsonProperty(PropertyName = "start")]
        public string Start { get; set; }
        
        /// <summary>
        /// End time.
        /// </summary>
        [JsonProperty(PropertyName = "end")]
        public string End { get; set; }
    }
}