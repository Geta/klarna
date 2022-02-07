using System.Text.Json.Serialization;
using Klarna.Common.Models;

namespace Klarna.Payments.Models
{
    public class PersonalInformationSession
    {
        [JsonPropertyName("billing_address")]
        public OrderManagementAddressInfo BillingAddress { get; set; }
        
        [JsonPropertyName("shipping_address")]
        public OrderManagementAddressInfo ShippingAddress { get; set; }
        
        [JsonPropertyName("customer")]
        public Customer Customer { get; set; }
    }
}