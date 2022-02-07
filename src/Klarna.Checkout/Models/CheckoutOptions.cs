using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Klarna.Checkout.Models
{
    public class CheckoutOptions
    {
        /// <summary>
        /// Acquiring channel for the order. Use MOTO for &quot;Mail Order Telephone Order&quot; or ECOMMERCE for &quot;E-commerce&quot;. Default : ECOMMERCE
        /// </summary>
        [JsonPropertyName("acquiring_channel")]
        public string AcquiringChannel { get; set; }
        /// <summary>
        /// If true, the consumer can enter different billing and shipping addresses. Default: false
        /// </summary>
        [JsonPropertyName("allow_separate_shipping_address")]
        public bool AllowSeparateShippingAddress { get; set; }
        /// <summary>
        /// CSS hex color, e.g. &quot;#FF9900&quot;
        /// </summary>
        [JsonPropertyName("color_button")]
        public string ColorButton { get; set; }
        /// <summary>
        /// CSS hex color, e.g. &quot;#FF9900&quot;
        /// </summary>
        [JsonPropertyName("color_button_text")]
        public string ColorButtonText { get; set; }
        /// <summary>
        /// CSS hex color, e.g. &quot;#FF9900&quot;
        /// </summary>
        [JsonPropertyName("color_checkbox")]
        public string ColorCheckbox { get; set; }
        /// <summary>
        /// CSS hex color, e.g. &quot;#FF9900&quot;
        /// </summary>
        [JsonPropertyName("color_checkbox_checkmark")]
        public string ColorCheckboxCheckmark { get; set; }
        /// <summary>
        /// CSS hex color, e.g. &quot;#FF9900&quot;
        /// </summary>
        [JsonPropertyName("colorheader")]
        public string ColorHeader { get; set; }
        /// <summary>
        /// CSS hex color, e.g. &quot;#FF9900&quot;
        /// </summary>
        [JsonPropertyName("color_link")]
        public string ColorLink { get; set; }
        /// <summary>
        /// If true, the consumer cannot skip date of birth. Default: false
        /// </summary>
        [JsonPropertyName("date_of_birth_mandatory")]
        public bool DateOfBirthMandatory { get; set; }
        /// <summary>
        /// A message that will be presented on the confirmation page under the headline &quot;Delivery&quot;.
        /// </summary>
        [JsonPropertyName("shipping_details")]
        public string ShippingDetails { get; set; }
        /// <summary>
        /// If specified to false, title becomes optional. Only available for orders for country GB.
        /// </summary>
        [JsonPropertyName("title_mandatory")]
        public bool TitleMandatory { get; set; }
        /// <summary>
        /// Additional merchant defined checkbox. e.g. for Newsletter opt-in.
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("additional_checkbox")]
        public AdditionalCheckbox AdditionalCheckbox { get; set; }
        /// <summary>
        /// Additional merchant defined field. e.g. for purchases that MUST have a national insurance number.
        /// </summary>
        [JsonPropertyName("national_identification_number_mandatory")]
        public bool NationalIdentificationNumberMandatory { get; set; }
        /// <summary>
        /// Additional merchant defined field. e.g. Extra terms and conditions to show.
        /// </summary>
        [JsonPropertyName("additional_merchant_terms")]
        public string AdditionalMerchantTerms { get; set; }
        /// <summary>
        /// Border radius.
        /// </summary>
        [JsonPropertyName("radius_border")]
        public string RadiusBorder { get; set; }
        /// <summary>
        /// A list of allowed customer types. Supported types: person &amp; organization
        /// </summary>
        [JsonPropertyName("allowed_customer_types")]
        public ICollection<string> AllowedCustomerTypes { get; set; }
        /// <summary>
        /// If true, the Order Detail subtotals view is expanded. Default: false
        /// </summary>
        [JsonPropertyName("show_subtotal_detail")]
        public bool ShowSubtotalDetail { get; set; }
        /// <summary>
        /// Additional merchant defined checkboxes. e.g. for Newsletter opt-in.
        /// </summary>
        [JsonPropertyName("additional_checkboxes")]
        public ICollection<AdditionalCheckboxV2> AdditionalCheckboxes { get; set; }
        /// <summary>
        /// If true, validate callback must get a positive response to not stop purchase. Default: false.
        /// </summary>
        [JsonPropertyName("require_validate_callback_success")]
        public bool RequireValidateCallbackSuccess { get; set; }
        /// <summary>
        /// If true, VAT is not included in total price
        /// </summary>
        [JsonPropertyName("vat_excluded")]
        public bool VatExcluded { get; set; }
        /// <summary>
        /// Enable verification of National Identification Numbers in Sweden and Norway. This option also make the national identification number mandatory.
        /// </summary>
        [JsonPropertyName("verify_national_identification_number")]
        public bool VerifyNationalIdentificationNumber { get; set; }
        /// <summary>
        /// If true, a optional VAT registration number field will be shown in the address form. Only applies for b2b orders.
        /// </summary>
        [JsonPropertyName("show_vat_registration_number_field")]
        public bool ShowVatRegistrationNumberField { get; set; }
    }
}
