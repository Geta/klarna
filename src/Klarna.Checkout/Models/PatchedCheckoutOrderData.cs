using System.Collections.Generic;
using Newtonsoft.Json;

namespace Klarna.Checkout.Models
{
    public class PatchedCheckoutOrderData : Rest.Models.CheckoutOrderData
    {
        [JsonProperty("selected_shipping_option")]
        public ShippingOption SelectedShippingOption { get; set; }

        [JsonProperty("shipping_options")]
        public IEnumerable<ShippingOption> ShippingOptions { get; set; }
    }
}
