using Newtonsoft.Json;

namespace Klarna.Common.Models
{
    public class ShippingOptionCarrierProduct
    {
        /// <summary>
        /// Name of carrier product.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        /// <summary>
        /// Id of carrier product.
        /// </summary>
        [JsonProperty(PropertyName = "identifier")]
        public string Id { get; set; }
    }
}