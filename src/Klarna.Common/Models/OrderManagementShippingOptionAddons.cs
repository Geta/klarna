using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class OrderManagementShippingOptionAddons
    {
        /// <summary>
        /// The type of the add-on, e.g. sms or entry-code.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        /// <summary>
        /// The price of the add-on.
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("price")]
        public int Price { get; set; }
        
        /// <summary>
        /// The ID provided by the TMS.
        /// </summary>
        [JsonPropertyName("external_id")]
        public string ExternalId { get; set; }
        
        /// <summary>
        /// The text provided by the user
        /// </summary>
        [JsonPropertyName("user_input")]
        public string UserInput { get; set; }
    }
}