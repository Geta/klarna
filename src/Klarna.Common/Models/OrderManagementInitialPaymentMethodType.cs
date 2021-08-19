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
        PAY_LATER_BY_CARD,
        MOBILEPAY,
        OTHER,
        SWISH
    }
}
