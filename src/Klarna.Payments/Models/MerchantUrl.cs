using Newtonsoft.Json;

namespace Klarna.Payments.Models
{
    public class MerchantUrl
    {
        [JsonProperty("status_update")]
        public string StatusUpdate { get; set; }

        [JsonProperty("confirmation")]
        public string Confirmation { get; set; }
        [JsonProperty("notification")]
        public string Notification { get; set; }
        [JsonProperty("push")]
        public string Push { get; set; }
    }
}