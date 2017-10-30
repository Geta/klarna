namespace Klarna.Payments
{
    public static class Constants
    {
        public const string KlarnaPaymentSystemKeyword = Common.Constants.KlarnaSystemKeyword + "Payments";

        // Cart property fields
        public const string KlarnaSessionIdCartField = "KlarnaSessionIdCart";
        public const string KlarnaClientTokenCartField = "KlarnaClientTokenCart";
        public const string KlarnaAllowSharingOfPersonalInformationCartField = "KlarnaAllowSharingOfPersonalInformationCart";
        public const string CartOrderNumberTempCartField = "CartOrderNumberTempCart";

        // Other payment meta field
        public const string AuthorizationTokenPaymentField = "AuthorizationTokenPayment";
        public const string KlarnaConfirmationUrlPaymentField = "KlarnaConfirmationUrlPayment";
    }
}
