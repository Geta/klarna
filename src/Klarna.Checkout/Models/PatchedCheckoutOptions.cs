using System.Collections.Generic;
using Newtonsoft.Json;

namespace Klarna.Checkout.Models
{
    public class PatchedCheckoutOptions : Rest.Models.CheckoutOptions
    {
        [JsonProperty("require_validate_callback_success")]
        public bool RequireValidateCallbackSuccess { get; set; }

        [JsonProperty("title_mandatory")]
        public bool TitleMandatory { get; set; }

        [JsonProperty("show_subtotal_detail")]
        public bool ShowSubtotalDetail { get; set; }

        [JsonProperty("radius_border")]
        public string RadiusBorder { get; set; }

        [JsonProperty("shipping_details")]
        public string ShippingDetails { get; set; }

        [JsonProperty("additional_checkbox")]
        public AdditionalCheckbox AdditionalCheckbox { get; set; }
    }
}
