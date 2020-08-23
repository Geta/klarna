using Newtonsoft.Json;

namespace Klarna.Common.Models
{
    public class OrderManagementShippingOptionAddons
    {
        /// <summary>
        /// The type of the add-on, e.g. sms or entry-code.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        
        /// <summary>
        /// The price of the add-on.
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonProperty(PropertyName = "price")]
        public int Price { get; set; }
        
        /// <summary>
        /// The ID provided by the TMS.
        /// </summary>
        [JsonProperty(PropertyName = "external_id")]
        public string ExternalId { get; set; }
        
        /// <summary>
        /// The text provided by the user
        /// </summary>
        [JsonProperty(PropertyName = "user_input")]
        public string UserInput { get; set; }
    }
}