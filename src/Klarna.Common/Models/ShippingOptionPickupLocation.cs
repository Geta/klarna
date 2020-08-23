using Newtonsoft.Json;

namespace Klarna.Common.Models
{
    public class ShippingOptionPickupLocation
    {
        /// <summary>
        /// Id.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        
        /// <summary>
        /// Name of the location.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        
        /// <summary>
        /// Location address.
        /// </summary>
        [JsonProperty(PropertyName = "address")]
        public CheckoutAddressInfo Address { get; set; }
    }
}