using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Klarna.Payments.Models
{
    public class Customer
    {
        /// <summary>
        /// ISO 8601 date. The customer date of birth
        /// </summary>
        [JsonPropertyName("date_of_birth")]
        public string DateOfBirth { get; set; }
        /// <summary>
        /// The customer's title
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }
        /// <summary>
        /// The customer gender
        /// </summary>
        [JsonPropertyName("gender")]
        public string Gender { get; set; }
        /// <summary>
        /// Last four digits for customer social security number
        /// </summary>
        [JsonPropertyName("last_four_ssn")]
        public string LastFourSsn { get; set; }
        /// <summary>
        /// The customer's national identification number
        /// </summary>
        [JsonPropertyName("national_identification_number")]
        public string NationalIdentificationNumber { get; set; }
        /// <summary>
        /// Type
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
        /// <summary>
        ///  VAT id
        /// </summary>
        [JsonPropertyName("vat_id")]
        public string VatId { get; set; }
        /// <summary>
        /// Organization entity type
        /// </summary>
        [JsonPropertyName("organization_registration_id")]
        public string OrganizationRegistrationId { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        [JsonPropertyName("organization_entity_type")]
        public PaymentCustomerOrganizationEntityType OrganizationEntityType { get; set; }
    }
}