using Klarna.Rest.Models;
using Newtonsoft.Json;

namespace Klarna.Payments.Models
{
    public class PersonalInformationSession
    {
        [JsonProperty("billing_address")]
        public Address BillingAddress { get; set; }
        [JsonProperty("shipping_address")]
        public Address ShippingAddress { get; set; }
        [JsonProperty("customer")]
        public Customer Customer { get; set; }
    }
}