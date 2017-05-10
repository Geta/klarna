using System.Collections.Generic;
using Klarna.Rest.Models;
using Newtonsoft.Json;

namespace Klarna.Checkout.Models
{
    public class AddressUpdateResponse
    {
        [JsonProperty("order_amount")]
        public int? OrderAmount { get; set; }

        [JsonProperty("order_tax_amount")]
        public int? OrderTaxAmount { get; set; }

        [JsonProperty("merchant_data")]
        public string MerchantData { get; private set; }

        [JsonProperty("order_lines")]
        public IList<OrderLine> OrderLines { get; set; }

        [JsonProperty("shipping_options")]
        public IEnumerable<ShippingOption> ShippingOptions { get; set; }

        [JsonProperty("attachment")]
        public Attachment Attachment { get; set; }

        [JsonProperty("purchase_currency")]
        public string PurchaseCurrency { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }
    }
}
