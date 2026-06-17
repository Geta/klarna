namespace Klarna.Common.Models
{
    public enum OrderManagementInitialPaymentMethodType
    {
        INVOICE,
        FIXED_AMOUNT,
        FIXED_AMOUNT_BY_CARD, // (non Australia region)
        PAY_LATER_IN_PARTS, // (Australia region)
        ACCOUNT,
        DIRECT_DEBIT,
        CARD,
        BANK_TRANSFER,
        PAY_IN_X,
        INVOICE_BUSINESS,
        DEFERRED_INTEREST,
        FIXED_SUM_CREDIT,
        PAY_BY_CARD,
        PAY_LATER_BY_CARD,
        MOBILEPAY,
        OTHER,
        SWISH,
        APPLE_PAY_CARD,
        GOOGLE_PAY_CARD,
        CARTES_BANCAIRES,
        BLIK,
        TWINT,
        BANCONTACT,
        APPLE_PAY_MASTERCARD,
        APPLE_PAY_VISA,
        GOOGLE_PAY_MASTERCARD,
        GOOGLE_PAY_VISA,
        KLARNA_PAY_IN_INSTALLMENTS,
        KLARNA_PAY_LATER,
        KLARNA_PAY_NOW,
        MASTERCARD_CREDIT,
        MASTERCARD_DEBIT,
        UNKNOWN_CARD,
        VISA_CREDIT,
        VISA_DEBIT,
        VIPPS,
    }
}
