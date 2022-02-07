using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Klarna.Common.Models
{
    public class OrderManagementShippingInfo
    {
        /// <summary>
        /// Name of the shipping company (as specific as possible). Maximum 100 characters. Example: 'DHL US' and not only 'DHL'
        /// </summary>
        [JsonPropertyName("shipping_company")]
        public string ShippingCompany { get; set; }
        /// <summary>
        /// Shipping method. Allowed values matches (PickUpStore|Home|BoxReg|BoxUnreg|PickUpPoint|Own|Postal|DHLPackstation|Digital)
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonPropertyName("shipping_method")]
        public OrderManagementShippingMethod ShippingMethod { get; set; }
        /// <summary>
        /// Tracking number for the shipment. Maximum 100 characters.
        /// </summary>
        [JsonPropertyName("tracking_number")]
        public string TrackingNumber { get; set; }
        /// <summary>
        /// URI where the customer can track their shipment. Maximum 1024 characters.
        /// </summary>
        [JsonPropertyName("tracking_uri")]
        public string TrackingUri { get; set; }
        /// <summary>
        /// Name of the shipping company for the return shipment (as specific as possible). Maximum 100 characters. Example: 'DHL US' and not only 'DHL'
        /// </summary>
        [JsonPropertyName("return_shipping_company")]
        public string ReturnShippingCompany { get; set; }
        /// <summary>
        /// Tracking number for the return shipment. Maximum 100 characters.
        /// </summary>
        [JsonPropertyName("return_tracking_number")]
        public string ReturnTrackingNumber { get; set; }
        /// <summary>
        /// URL where the customer can track the return shipment. Maximum 1024 characters.
        /// </summary>
        [JsonPropertyName("return_tracking_uri")]
        public string ReturnTrackingUri { get; set; }
    }
}
