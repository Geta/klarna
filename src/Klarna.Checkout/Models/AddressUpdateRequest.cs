using System.Collections.Generic;
using System.Text.Json.Serialization;
using Klarna.Common.Models;

namespace Klarna.Checkout.Models
{
    public class AddressUpdateRequest
    {
        [JsonPropertyName("order_amount")]
        public int? OrderAmount { get; set; }

        [JsonPropertyName("order_tax_amount")]
        public int? OrderTaxAmount { get; set; }

        [JsonPropertyName("order_lines")]
        public IList<OrderLine> OrderLines { get; set; }

        [JsonPropertyName("billing_address")]
        public CheckoutAddressInfo BillingAddress { get; set; }

        [JsonPropertyName("shipping_address")]
        public CheckoutAddressInfo ShippingAddress { get; set; }

        [JsonPropertyName("selected_shipping_option")]
        public ShippingOption SelectedShippingOption { get; set; }

        [JsonPropertyName("purchase_currency")]
        public string PurchaseCurrency { get; set; }
    }
}
