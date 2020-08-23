using Newtonsoft.Json;

namespace Klarna.Common.Models
{
    public class DeliveryDetails
    {
        /// <summary>
        /// Carrier product name.
        /// </summary>
        [JsonProperty(PropertyName = "carrier")]
        public string Carrier { get; set; }
        
        /// <summary>
        /// Type of shipping class.
        /// </summary>
        [JsonProperty(PropertyName = "class")]
        public string Class { get; set; }
        
        /// <summary>
        /// Upstream carrier product.
        /// </summary>
        [JsonProperty(PropertyName = "product")]
        public ShippingOptionCarrierProduct Product { get; set; }
        
        /// <summary>
        /// The selected location for this shipping option.
        /// </summary>
        [JsonProperty(PropertyName = "pickup_location")]
        public ShippingOptionPickupLocation PickupLocation { get; set; }
        
        /// <summary>
        /// The selected timeslot for this shipping option.
        /// </summary>
        [JsonProperty(PropertyName = "timeslot")]
        public ShippingOptionTimeslot Timeslot { get; set; }
    }
}