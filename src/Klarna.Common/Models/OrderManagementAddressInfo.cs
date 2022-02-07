﻿using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class OrderManagementAddressInfo
    {
        /// <summary>
        /// Given name. Maximum 100 characters.
        /// </summary>
        [JsonPropertyName("given_name")]
        public string GivenName { get; set; }
        /// <summary>
        /// Family name. Maximum 100 characters.
        /// </summary>
        [JsonPropertyName("family_name")]
        public string FamilyName { get; set; }
        /// <summary>
        /// Title. Between 0 and 20 characters.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }
        /// <summary>
        /// First line of street address. Maximum 100 characters.
        /// </summary>
        [JsonPropertyName("street_address")]
        public string StreetAddress { get; set; }
        /// <summary>
        /// Second line of street address. Maximum 100 characters.
        /// </summary>
        [JsonPropertyName("street_address2")]
        public string StreetAddress2 { get; set; }
        /// <summary>
        /// Postcode. Maximum 10 characters.
        /// </summary>
        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; }
        /// <summary>
        /// City. Maximum 200 characters.
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; }
        /// <summary>
        /// State/Region. Required for some countries. Maximum 200 characters.
        /// </summary>
        [JsonPropertyName("region")]
        public string Region { get; set; }
        /// <summary>
        /// Country. ISO 3166 alpha-2.
        /// </summary>
        [JsonPropertyName("country")]
        public string Country { get; set; }
        /// <summary>
        /// E-mail address. Maximum 100 characters.
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; }
        /// <summary>
        /// Phone number. Maximum 100 characters.
        /// </summary>
        [JsonPropertyName("phone")]
        public string Phone { get; set; }
    }
}
