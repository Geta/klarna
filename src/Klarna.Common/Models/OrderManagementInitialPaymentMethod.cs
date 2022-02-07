using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Klarna.Common.Models
{
    public class OrderManagementInitialPaymentMethod
    {
        /// <summary>
        /// The type of the initial payment method.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonPropertyName("type")]
        public OrderManagementInitialPaymentMethodType Type { get; set; }
        /// <summary>
        /// The description of the initial payment method.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// The number of installments (if applicable).
        /// </summary>
        [JsonPropertyName("number_of_installments")]
        public int NumberOfInstallments { get; set; }
    }
}
