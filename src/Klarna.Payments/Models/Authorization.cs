using Newtonsoft.Json;

namespace Klarna.Payments.Models
{
    public class Authorization
    {
        [JsonProperty("purchase_country")]
        public string PurchaseCountry { get; set; }
        [JsonProperty("purchase_currency")]
        public string PurchaseCurrency { get; set; }
        [JsonProperty("locale")]
        public string Locale { get; set; }
        [JsonProperty("billing_address")]
        public Address BillingAddress { get; set; }
        [JsonProperty("shipping_address")]
        public Address ShippingAddress { get; set; }
        [JsonProperty("order_amount")]
        public int OrderAmount { get; set; }
        [JsonProperty("order_tax_amount")]
        public int OrderTaxAmount { get; set; }
        [JsonProperty("order_lines")]
        public OrderLine[] OrderLines { get; set; }
        [JsonProperty("customer")]
        public Customer Customer { get; set; }
        [JsonProperty("merchant_reference1")]
        public string MerchantReference1 { get; set; }
        [JsonProperty("merchant_reference2")]
        public string MerchantReference2 { get; set; }
        [JsonProperty("merchant_data")]
        public string MerchantData { get; set; }
        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
