using Newtonsoft.Json;

namespace Klarna.Payments.Models
{
    public class Address
    {
        [JsonProperty("given_name")]
        public string GivenName { get; set; }
        [JsonProperty("family_name")]
        public string FamilyName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("street_address")]
        public string StreetAddress { get; set; }
        [JsonProperty("street_address2")]
        public string StreetAddress2 { get; set; }
        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("region")]
        public string Region { get; set; }
        [JsonProperty("phone")]
        public string Phone { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
    }
}