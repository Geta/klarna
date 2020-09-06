using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Klarna.Common.Models
{
    public class OrderManagementInitialPaymentMethod
    {
        /// <summary>
        /// The type of the initial payment method.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "type")]
        public OrderManagementInitialPaymentMethodType Type { get; set; }
        /// <summary>
        /// The description of the initial payment method.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// The number of installments (if applicable).
        /// </summary>
        [JsonProperty(PropertyName = "number_of_installments")]
        public int NumberOfInstallments { get; set; }
    }
}
