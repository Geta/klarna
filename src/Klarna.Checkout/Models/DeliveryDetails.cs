using System.Text.Json.Serialization;

namespace Klarna.Checkout.Models
{
    public class DeliveryDetails
    {
        /// <summary>
        /// Carrier product name.
        /// </summary>
        [JsonPropertyName("carrier")]
        public string Carrier { get; set; }
        
        /// <summary>
        /// Type of shipping class.
        /// </summary>
        [JsonPropertyName("class")]
        public string Class { get; set; }
        
        /// <summary>
        /// Upstream carrier product.
        /// </summary>
        [JsonPropertyName("product")]
        public ShippingOptionCarrierProduct Product { get; set; }
        
        /// <summary>
        /// The selected location for this shipping option.
        /// </summary>
        [JsonPropertyName("pickup_location")]
        public ShippingOptionPickupLocation PickupLocation { get; set; }
        
        /// <summary>
        /// The selected timeslot for this shipping option.
        /// </summary>
        [JsonPropertyName("timeslot")]
        public ShippingOptionTimeslot Timeslot { get; set; }
    }
}