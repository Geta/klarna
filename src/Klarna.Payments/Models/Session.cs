using Klarna.Common.Models;
using Klarna.Rest.Models;
using Newtonsoft.Json;

namespace Klarna.Payments.Models
{
    public class Session : PersonalInformationSession
    {
        [JsonProperty("design")]
        public string Design { get; set; }
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
        public PatchedOrderLine[] OrderLines { get; set; }
        [JsonProperty("merchant_urls")]
        public MerchantUrl MerchantUrl { get; set; }
        [JsonProperty("merchant_reference1")]
        public string MerchantReference1 { get; set; }
        [JsonProperty("merchant_reference2")]
        public string MerchantReference2 { get; set; }
        [JsonProperty("merchant_data")]
        public string MerchantData { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
        [JsonProperty("options")]
        public Options Options { get; set; }
        [JsonProperty("attachment")]
        public Attachment Attachment { get; set; }
    }
}
