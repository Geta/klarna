namespace Klarna.Payments
{
    public static class Constants
    {
        public const string KlarnaPaymentSystemKeyword = "KlarnaPayments";

        // Cart meta fields
        public const string KlarnaSessionIdField = "KlarnaSessionId";
        public const string KlarnaClientTokenField = "KlarnaClientToken";
        public const string KlarnaAllowSharingOfPersonalInformationField = "KlarnaAllowSharingOfPersonalInformationField";
        public const string CartOrderNumberTempField = "CartOrderNumberTemp";

        public const string ConfirmationUrlField = "ConfirmationUrl";
        public const string NotificationUrlField = "NotificationUrl";
        public const string SendProductAndImageUrlField = "SendProductAndImageUrl";
        public const string UseAttachmentsField = "UseAttachments";
        public const string KlarnaConfirmationUrlField = "KlarnaConfirmationUrl";
        public const string PreAssesmentCountriesField = "PreAssesmentCountries";
        
        // Payment fields
        public const string AuthorizationTokenPaymentMethodField = "AuthorizationTokenPaymentMethod";
    }
}
