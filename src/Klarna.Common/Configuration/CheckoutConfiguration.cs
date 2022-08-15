namespace Klarna.Common.Configuration
{
    public class CheckoutConfiguration : ConnectionConfiguration
    {
        public bool ShippingOptionsInIFrame { get; set; } = true;
        public bool AllowSeparateShippingAddress { get; set; }
        public bool DateOfBirthMandatory { get; set; }
        public string ShippingDetailsText { get; set; }
        public bool TitleMandatory { get; set; }
        public bool ShowSubtotalDetail { get; set; }
        public bool RequireValidateCallbackSuccess { get; set; } = true;
        public string AdditionalCheckboxText { get; set; }
        public bool AdditionalCheckboxDefaultChecked { get; set; }
        public bool AdditionalCheckboxRequired { get; set; }
        public bool SendShippingCountries { get; set; }
        public bool PrefillAddress { get; set; }
        public bool SendShippingOptionsPriorAddresses { get; set; } = true;
        public string ConfirmationUrl { get; set; }
        public string TermsUrl { get; set; }
        public string CancellationTermsUrl { get; set; }
        public string CheckoutUrl { get; set; }
        public string PushUrl { get; set; }
        public string NotificationUrl { get; set; }
        public string ShippingOptionUpdateUrl { get; set; }
        public string AddressUpdateUrl { get; set; }
        public string OrderValidationUrl { get; set; }
        public string WidgetButtonColor { get; set; }
        public string WidgetButtonTextColor { get; set; }
        public string WidgetCheckboxColor { get; set; }
        public string WidgetCheckboxCheckmarkColor { get; set; }
        public string WidgetHeaderColor { get; set; }
        public string WidgetLinkColor { get; set; }
        public string WidgetBorderRadius { get; set; }
        public bool SendProductAndImageUrl { get; set; } = true;
    }
}