using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Klarna.Common.Models
{
    public class OrderLine
    {
        /// <summary>
        /// Order line type.
        /// </summary>
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        [JsonPropertyName("type")]
        public OrderLineType Type { get; set; }
        /// <summary>
        /// Article number, SKU or similar.
        /// </summary>
        [JsonPropertyName("reference")]
        public string Reference { get; set; }
        /// <summary>
        /// Descriptive item name.
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// Non-negative. The item quantity.
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        /// <summary>
        /// Unit used to describe the quantity, e.g. kg, pcs... If defined has to be 1-8 characters
        /// </summary>
        [JsonPropertyName("quantity_unit")]
        public string QuantityUnit { get; set; }
        /// <summary>
        /// Minor units. Includes tax, excludes discount. (max value: 100000000)
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("unit_price")]
        public int UnitPrice { get; set; }
        /// <summary>
        /// Non-negative. In percent, two implicit decimals. I.e 2500 = 25%. (max value: 10000)
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("tax_rate")]
        public int TaxRate { get; set; }
        /// <summary>
        /// Includes tax and discount. Must match (quantity * unit_price) - total_discount_amount within ±quantity. (max value: 100000000)
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("total_amount")]
        public int TotalAmount { get; set; }
        /// <summary>
        /// Non-negative minor units. Includes tax.
        /// </summary>
        [JsonPropertyName("total_discount_amount")]
        public int TotalDiscountAmount { get; set; }
        /// <summary>
        /// Must be within ±1 of total_amount - total_amount * 10000 / (10000 + tax_rate). Negative when type is discount.
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("total_tax_amount")]
        public int TotalTaxAmount { get; set; }
        /// <summary>
        /// Pass through field. (max 255 characters)
        /// </summary>
        [JsonPropertyName("merchant_data")]
        public string MerchantData { get; set; }
        /// <summary>
        /// URL to the product page that can be later embedded in communications between Klarna and the customer. (max 1024 characters)
        /// </summary>
        [JsonPropertyName("product_url")]
        public string ProductUrl { get; set; }
        /// <summary>
        /// URL to an image that can be later embedded in communications between Klarna and the customer. (max 1024 characters)
        /// </summary>
        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }
        /// <summary>
        /// Additional information identifying an item
        /// </summary>
        [JsonPropertyName("product_identifiers")]
        public ProductIdentifiers ProductIdentifiers { get; set; }
    }
}
