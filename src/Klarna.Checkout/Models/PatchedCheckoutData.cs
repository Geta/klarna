using Newtonsoft.Json;

namespace Klarna.Checkout.Models
{
    public class PatchedCheckoutData : Rest.Models.CheckoutOrderData
    {
        [JsonProperty("selected_shipping_option")]
        public SelectedShippingOption SelectedShippingOption { get; set; }
    }
}
