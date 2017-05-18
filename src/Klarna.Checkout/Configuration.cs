namespace Klarna.Checkout
{
    public class Configuration
    {
        public bool ShippingOptionsInIFrame { get; set; }
        public bool AllowSeparateShippingAddress { get; set; }
        public bool DateOfBirthMandatory { get; set; }
        public string ShippingDetailsText { get; set; }
        public bool TitleMandatory { get; set; }
        public bool ShowSubtotalDetail { get; set; }
        public bool RequireValidateCallbackSuccess { get; set; }
        public string AdditionalCheckboxText { get; set; }
        public bool AdditionalCheckboxDefaultChecked { get; set; }
        public bool AdditionalCheckboxRequired { get; set; }
        public string ConfirmationUrl { get; set; }
        public string TermsUrl { get; set; }
        public string CheckoutUrl { get; set; }
        public string PushUrl { get; set; }
        public string NotificationUrl { get; set; }
        public string ShippingOptionUpdateUrl { get; set; }
        public string AddressUpdateUrl { get; set; }
        public string OrderValidationUrl { get; set; }
    }
}