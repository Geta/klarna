using System.Text.Json.Serialization;
using Klarna.Common.Models;

namespace Klarna.Payments.Models
{
    public class Authorization
    {
        [JsonPropertyName("purchase_country")]
        public string PurchaseCountry { get; set; }
        [JsonPropertyName("purchase_currency")]
        public string PurchaseCurrency { get; set; }
        [JsonPropertyName("locale")]
        public string Locale { get; set; }
        [JsonPropertyName("billing_address")]
        public OrderManagementAddressInfo BillingAddress { get; set; }
        [JsonPropertyName("shipping_address")]
        public OrderManagementAddressInfo ShippingAddress { get; set; }
        [JsonPropertyName("order_amount")]
        public int OrderAmount { get; set; }
        [JsonPropertyName("order_tax_amount")]
        public int OrderTaxAmount { get; set; }
        [JsonPropertyName("order_lines")]
        public OrderLine[] OrderLines { get; set; }
        [JsonPropertyName("customer")]
        public Customer Customer { get; set; }
        [JsonPropertyName("merchant_reference1")]
        public string MerchantReference1 { get; set; }
        [JsonPropertyName("merchant_reference2")]
        public string MerchantReference2 { get; set; }
        [JsonPropertyName("merchant_data")]
        public string MerchantData { get; set; }
        [JsonPropertyName("body")]
        public string Body { get; set; }
    }
}
