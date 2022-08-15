using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class ErrorMessage
    {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        [JsonPropertyName("error_code")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets error messages.
        /// </summary>
        [JsonPropertyName("error_messages")]
        public string[] ErrorMessages { get; set; }

        /// <summary>
        /// Gets or sets correlation id.
        /// <para>The correlation id may be requested by merchant support to facilitate support inquiries.</para>
        /// </summary>
        [JsonPropertyName("correlation_id")]
        public string CorrelationId { get; set; }
    }
}
