using Newtonsoft.Json;

namespace Klarna.Payments.Models
{
    public class Customer : Klarna.Rest.Models.Customer
    {
        [JsonProperty("gender")]
        public string Gender { get; set; }
        [JsonProperty("last_four_ssn")]
        public string LastFourSsn{ get; set; }
    }
}