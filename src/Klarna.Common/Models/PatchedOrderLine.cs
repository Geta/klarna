using Newtonsoft.Json;

namespace Klarna.Common.Models
{
    public class PatchedOrderLine : Rest.Core.Model.OrderLine
    {
        [JsonProperty("image_url")]
        public string ProductImageUrl { get; set; }
    }
}