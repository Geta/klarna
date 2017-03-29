using Newtonsoft.Json;

namespace Klarna.Payments.Models
{
    public class CreateSessionRequest
    {
        [JsonProperty("purchase_country")]
        public string PurchaseCountry { get; set; }
        [JsonProperty("purchase_currency")]
        public string PurchaseCurrency { get; set; }
        [JsonProperty("locale")]
        public string Locale { get; set; }
        [JsonProperty("order_amount")]
        public int OrderAmount { get; set; }
        [JsonProperty("order_tax_amount")]
        public int OrderTaxAmount { get; set; }
        [JsonProperty("order_lines")]
        public OrderLines[] OrderLines { get; set; }
    }

    public class OrderLines
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("reference")]
        public string Reference { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
        [JsonProperty("unit_price")]
        public int UnitPrice { get; set; }
        [JsonProperty("tax_rate")]
        public int TaxRate { get; set; }
        [JsonProperty("total_amount")]
        public int TotalAmount { get; set; }
        [JsonProperty("total_discount_amount")]
        public int TotalDiscountAmount { get; set; }
        [JsonProperty("total_tax_amount")]
        public int TotalTaxAmount { get; set; }
    }

}
