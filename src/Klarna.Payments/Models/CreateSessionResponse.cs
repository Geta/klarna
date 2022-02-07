using System.Text.Json.Serialization;

namespace Klarna.Payments.Models
{
    public class CreateSessionResponse
    {
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; }

        [JsonPropertyName("client_token")]
        public string ClientToken { get; set; }

        [JsonPropertyName("payment_method_categories")]
        public PaymentMethodCategory[] PaymentMethodCategories { get; set; }
    }
}
