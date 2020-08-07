using Newtonsoft.Json;

namespace Klarna.Common.Models
{
    public class MerchantRequestedAdditionalCheckbox
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "checked")]
        public bool Checked { get; set; }
    }
}
