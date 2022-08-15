using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class OrderManagementCustomerAddresses
    {
        /// <summary>
        /// Customer shipping address.
        /// </summary>
        [JsonPropertyName("shipping_address")]
        public OrderManagementAddressInfo ShippingAddress { get; set; }
        /// <summary>
        /// Customer billing address.
        /// </summary>
        [JsonPropertyName("billing_address")]
        public OrderManagementAddressInfo BillingAddress { get; set; }
    }
}
