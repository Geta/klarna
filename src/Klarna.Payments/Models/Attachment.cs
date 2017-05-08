using Newtonsoft.Json;

namespace Klarna.Payments.Models
{
    public class Attachment
    {
        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }
}