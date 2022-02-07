using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class OrderManagementShippingOptionLocation
    {
        /// <summary>
        /// The location id.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        /// <summary>
        /// The display name of the location.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// The price for this location.
        /// </summary>
        [JsonPropertyName("price")]
        public int Price { get; set; }
        /// <summary>
        /// The address of the location.
        /// </summary>
        [JsonPropertyName("address")]
        public OrderManagementShippingOptionLocationAddress Address { get; set; }
    }
}