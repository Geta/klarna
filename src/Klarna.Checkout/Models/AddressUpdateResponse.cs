using System.Collections.Generic;
using System.Text.Json.Serialization;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using Klarna.Common.Models;

namespace Klarna.Checkout.Models
{
    public class AddressUpdateResponse
    {
        [JsonPropertyName("order_amount")]
        public int? OrderAmount { get; set; }

        [JsonPropertyName("order_tax_amount")]
        public int? OrderTaxAmount { get; set; }

        [JsonPropertyName("merchant_data")]
        public string MerchantData { get; private set; }

        [JsonPropertyName("order_lines")]
        public IList<OrderLine> OrderLines { get; set; }

        [JsonPropertyName("shipping_options")]
        public IEnumerable<ShippingOption> ShippingOptions { get; set; }

        [JsonPropertyName("attachment")]
        public Attachment Attachment { get; set; }

        [JsonPropertyName("purchase_currency")]
        public string PurchaseCurrency { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonIgnore]
        public Dictionary<ILineItem, List<ValidationIssue>> ValidationIssues { get; set; }
        
        [JsonIgnore]
        public IEnumerable<RewardDescription> RewardDescriptions { get; set; }

    }
}
