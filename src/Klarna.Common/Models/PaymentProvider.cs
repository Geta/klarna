using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class PaymentProvider
    {
        /// <summary>
        /// The name of the payment provider. (max 255 characters)
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// URL to redirect to. (must be https, min 7, max 2000 characters)
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("redirect_url")]
        public string RedirectUrl { get; set; }
        /// <summary>
        /// URL to an image to display. (must be https, max 2000 characters)
        /// </summary>
        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }
        /// <summary>
        /// Minor units. Includes tax.
        /// </summary>
        [JsonPropertyName("fee")]
        public int Fee { get; set; }
        /// <summary>
        /// Description (max 500 characters)
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }
        /// <summary>
        /// If specified, limits the method to the listed countries (ISO 3155 alpha-2).
        /// </summary>
        [JsonPropertyName("countries")]
        public ICollection<string> Countries { get; set; }
        /// <summary>
        /// Controls label of buy button:
        ///  * continue
        ///  * complete
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; set; }
    }
}
