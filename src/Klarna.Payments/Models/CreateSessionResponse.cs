using Newtonsoft.Json;

namespace Klarna.Payments.Models
{
    public class CreateSessionResponse
    {
        [JsonProperty("session_id")]
        public string SessionId { get; set; }
        [JsonProperty("client_token")]
        public string ClientToken { get; set; }
    }
}
