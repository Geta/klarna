﻿namespace Klarna.Payments
{
    public static class Constants
    {
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
