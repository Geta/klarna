using System.Text.Json.Serialization;

namespace Klarna.Checkout.Models
{
    public class CheckoutAddressInfo
    {
        /// <summary>
        /// Gets organization name
        /// </summary>
        [JsonPropertyName("organization_name")]
        public string OrganizationName { get; set; }

        /// <summary>
        /// Gets Shipping Reference
        /// </summary>
        [JsonPropertyName("reference")]
        public string Reference { get; set; }

        /// <summary>
        /// Gets attention
        /// </summary>
        [JsonPropertyName("attention")]
        public string Attention { get; set; }

        /// <summary>
        /// Gets firstname
        /// </summary>
        [JsonPropertyName("given_name")]
        public string GivenName { get; set; }

        /// <summary>
        /// Gets lastname
        /// </summary>
        [JsonPropertyName("family_name")]
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets e-mail
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; }

        /// <summary>
        /// Title.
        /// Valid values for UK:
        ///   Mr
        ///   Ms
        ///   Mrs
        ///   Miss
        /// Valid values for DACH:
        ///   Herr
        ///   Frau
        /// Valid values for NL:
        ///   Dhr.
        ///   Mevr.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }
        /// <summary>
        /// Street address, first line.
        /// </summary>
        [JsonPropertyName("street_address")]
        public string StreetAddress { get; set; }
        /// <summary>
        /// Street address, second line.
        /// </summary>
        [JsonPropertyName("street_address2")]
        public string StreetAddress2 { get; set; }
        /// <summary>
        /// Street name. Only applicable in DE/AT/NL. Do not combine with street_address. See streetNumber.
        /// </summary>
        [JsonPropertyName("street_name")]
        public string StreetName { get; set; }
        /// <summary>
        /// Street number. Only applicable in DE/AT/NL. Do not combine with street_address. See streetName.
        /// </summary>
        [JsonPropertyName("street_number")]
        public string StreetNumber { get; set; }

        /// <summary>
        /// House extension. Only applicable in NL
        /// </summary>
        [JsonPropertyName("house_extension")]
        public string HouseExtension { get; set; }

        /// <summary>
        /// Postal/post code.
        /// </summary>
        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }
        /// <summary>
        /// State or Region.
        /// </summary>
        [JsonPropertyName("region")]
        public string Region { get; set; }
        /// <summary>
        /// Phone number.
        /// </summary>
        [JsonPropertyName("phone")]
        public string Phone { get; set; }
        /// <summary>
        /// ISO 3166 alpha-2 Country.
        /// </summary>
        [JsonPropertyName("country")]
        public string Country { get; set; }
        /// <summary>
        /// Care of.
        /// </summary>
        [JsonPropertyName("care_of")]
        public string CareOf { get; set; }
    }
}
