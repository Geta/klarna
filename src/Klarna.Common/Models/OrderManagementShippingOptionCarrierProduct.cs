using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class OrderManagementShippingOptionCarrierProduct
    {
        /// <summary>
        /// Name of carrier product.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// Id of carrier product.
        /// </summary>
        [JsonPropertyName("identifier")]
        public string Id { get; set; }
    }
}