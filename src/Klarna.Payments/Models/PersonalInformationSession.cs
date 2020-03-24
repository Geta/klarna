using Klarna.Rest.Core.Model;
using Newtonsoft.Json;

namespace Klarna.Payments.Models
{
    public class PersonalInformationSession
    {
        [JsonProperty("billing_address")]
        public OrderManagementAddressInfo BillingAddress { get; set; }
        [JsonProperty("shipping_address")]
        public OrderManagementAddressInfo ShippingAddress { get; set; }
        [JsonProperty("customer")]
        public Customer Customer { get; set; }
    }
}