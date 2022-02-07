using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class OrderManagementShippingOptionLocationAddress
    {
        /// <summary>
        /// The street address.
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("street_address")]
        public string StreetAddress { get; set; }
        /// <summary>
        /// Additional street address.
        /// </summary>
        [JsonPropertyName("street_address__2")]
        public string StreetAddress2 { get; set; }
        /// <summary>
        /// Postal code.
        /// </summary>
        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; }
        /// <summary>
        /// City.
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; }
        /// <summary>
        /// Region.
        /// </summary>
        [JsonPropertyName("region")]
        public string Region { get; set; }
        /// <summary>
        /// Country.
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("country")]
        public string Country { get; set; }
    }
}