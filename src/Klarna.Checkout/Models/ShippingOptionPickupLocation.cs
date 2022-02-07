using System.Text.Json.Serialization;

namespace Klarna.Checkout.Models
{
    public class ShippingOptionPickupLocation
    {
        /// <summary>
        /// Id.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// Name of the location.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// Location address.
        /// </summary>
        [JsonPropertyName("address")]
        public CheckoutAddressInfo Address { get; set; }
    }
}