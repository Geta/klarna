using Newtonsoft.Json;

namespace Klarna.Checkout.Models
{
    public class SelectedShippingOption
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("promo")]
        public string Promo { get; set; }

        [JsonProperty("price")]
        public long Price { get; set; }

        [JsonProperty("tax_amount")]
        public long TaxAmount { get; set; }

        [JsonProperty("tax_rate")]
        public long TaxRate { get; set; }

        [JsonProperty("preselected")]
        public bool PreSelected { get; set; }

        [JsonProperty("shipping_method")]
        public string ShippingMethod { get; set; }
    }

}
