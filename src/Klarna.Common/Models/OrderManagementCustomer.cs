using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class OrderManagementCustomer
    {
        /// <summary>
        /// The customer date of birth. ISO 8601.
        /// </summary>
        [JsonPropertyName("date_of_birth")]
        public string DateOfBirth { get; set; }
        /// <summary>
        /// The customer national identification number.
        /// </summary>
        [JsonPropertyName("national_identification_number")]
        public string NationalIdentificationNumber { get; set; }
    }
}
