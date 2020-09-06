using Newtonsoft.Json;

namespace Klarna.Common.Models
{
    public class OrderManagementShippingOptionLocationAddress
    {
        /// <summary>
        /// The street address.
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonProperty(PropertyName = "street_address")]
        public string StreetAddress { get; set; }
        /// <summary>
        /// Additional street address.
        /// </summary>
        [JsonProperty(PropertyName = "street_address__2")]
        public string StreetAddress2 { get; set; }
        /// <summary>
        /// Postal code.
        /// </summary>
        [JsonProperty(PropertyName = "postal_code")]
        public string PostalCode { get; set; }
        /// <summary>
        /// City.
        /// </summary>
        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }
        /// <summary>
        /// Region.
        /// </summary>
        [JsonProperty(PropertyName = "region")]
        public string Region { get; set; }
        /// <summary>
        /// Country.
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }
    }
}