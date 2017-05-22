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
        public const string KlarnaConfirmationUrlField = "KlarnaConfirmationUrl";

        // Payment fields
        public const string AuthorizationTokenPaymentMethodField = "AuthorizationTokenPaymentMethod";
    }
}
