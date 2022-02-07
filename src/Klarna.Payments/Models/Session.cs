using System.Collections.Generic;
using System.Text.Json.Serialization;
using Klarna.Common.Models;

namespace Klarna.Payments.Models
{
    public class Session : PersonalInformationSession
    {
        [JsonPropertyName("design")]
        public string Design { get; set; }

        [JsonPropertyName("purchase_country")]
        public string PurchaseCountry { get; set; }

        [JsonPropertyName("purchase_currency")]
        public string PurchaseCurrency { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonPropertyName("order_amount")]
        public int OrderAmount { get; set; }

        [JsonPropertyName("order_tax_amount")]
        public int OrderTaxAmount { get; set; }

        [JsonPropertyName("order_lines")]
        public OrderLine[] OrderLines { get; set; }

        [JsonPropertyName("merchant_urls")]
        public MerchantUrl MerchantUrl { get; set; }

        [JsonPropertyName("merchant_reference1")]
        public string MerchantReference1 { get; set; }

        [JsonPropertyName("merchant_reference2")]
        public string MerchantReference2 { get; set; }

        [JsonPropertyName("merchant_data")]
        public string MerchantData { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("options")]
        public Options Options { get; set; }

        [JsonPropertyName("attachment")]
        public Attachment Attachment { get; set; }

        [JsonPropertyName("acquiring_channel")]
        public string AcquiringChannel { get; set; }

        [JsonPropertyName("auto_capture")]
        public bool AutoCapture { get; set; }

        [JsonPropertyName("customer_payment_method_ids")]
        public ICollection<string> CustomPaymentMethodIds { get; set; }
    }
}
