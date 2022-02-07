using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class OrderManagementCreateCapture
    {
        /// <summary>
        /// The captured amount in minor units.
        /// </summary>
        [JsonPropertyName("captured_amount")]
        public int CapturedAmount { get; set; }
        /// <summary>
        /// Description of the capture shown to the customer. Maximum 255 characters.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }
        /// <summary>
        /// Order lines for this capture. Maximum 1000 items.
        /// </summary>
        [JsonPropertyName("order_lines")]
        public ICollection<OrderLine> OrderLines { get; set; }
        /// <summary>
        /// Shipping information for this capture. Maximum 500 items.
        /// </summary>
        [JsonPropertyName("shipping_info")]
        public ICollection<OrderManagementShippingInfo> ShippingInfo { get; set; }
        /// <summary>
        /// Delay before the order will be shipped. Use for improving the customer experience regarding payments. This field is currently not returned when reading the order. Minimum: 0. Please note: to be able to submit values larger than 0, this has to be enabled in your merchant account. Please contact Klarna for further information.
        /// </summary>
        [JsonPropertyName("shipping_delay")]
        public int ShippingDelay { get; set; }
    }
}
