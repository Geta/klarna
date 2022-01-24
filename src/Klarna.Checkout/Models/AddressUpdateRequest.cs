
using System.Collections.Generic;
using Klarna.Common.Models;
using Newtonsoft.Json;

namespace Klarna.Checkout.Models
{
    public class AddressUpdateRequest
    {
        [JsonProperty("order_amount")]
        public int? OrderAmount { get; set; }

        [JsonProperty("order_tax_amount")]
        public int? OrderTaxAmount { get; set; }

        [JsonProperty("order_lines")]
        public IList<OrderLine> OrderLines { get; set; }

        [JsonProperty("billing_address")]
        public CheckoutAddressInfo BillingAddress { get; set; }

        [JsonProperty("shipping_address")]
        public CheckoutAddressInfo ShippingAddress { get; set; }

        [JsonProperty("selected_shipping_option")]
        public ShippingOption SelectedShippingOption { get; set; }

        [JsonProperty("purchase_currency")]
        public string PurchaseCurrency { get; set; }
    }
}
