using Newtonsoft.Json;

namespace Klarna.Common.Models
{
    public class PatchedOrderLine : Klarna.Rest.Models.OrderLine
    {
        [JsonProperty("product_url")]
        public string ProductUrl { get; set; }
        [JsonProperty("image_url")]
        public string ProductImageUrl { get; set; }
    }
}