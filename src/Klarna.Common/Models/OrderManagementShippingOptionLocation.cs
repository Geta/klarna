using Newtonsoft.Json;

namespace Klarna.Common.Models
{
    public class OrderManagementShippingOptionLocation
    {
        /// <summary>
        /// The location id.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        /// <summary>
        /// The display name of the location.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        
        /// <summary>
        /// The price for this location.
        /// </summary>
        [JsonProperty(PropertyName = "price")]
        public int Price { get; set; }
        /// <summary>
        /// The address of the location.
        /// </summary>
        [JsonProperty(PropertyName = "address")]
        public OrderManagementShippingOptionLocationAddress Address { get; set; }
    }
}