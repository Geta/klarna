using System.Text.Json.Serialization;
using Klarna.Common.Models;

namespace Klarna.Payments.Models
{
    public class ExpressButtonAddressInfo : OrderManagementAddressInfo
    {
        /// <summary>
        /// Since Express Button uses first_name and the rest given_name we make this private and set the GivenName property with the value.
        /// </summary>
        [JsonPropertyName("first_name")]
        private string FirstName { set => GivenName = value; }

        /// <summary>
        /// Since Express Button uses last_name and the rest family_name we make this private and set the FamilyName property with the value.
        /// </summary>
        [JsonPropertyName("last_name")]
        private string LastName { set => FamilyName = value; }

        /// <summary>
        /// ISO 8601 date. The customer date of birth
        /// </summary>
        [JsonPropertyName("date_of_birth")]
        public string DateOfBirth { get; set; }
    }
}