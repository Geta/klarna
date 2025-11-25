namespace Klarna.Common.Models
{
    // IMPORTANT: This list needs to be kept up to date with
    // https://docs.kustom.co/contents/api/order-management/orders/getorder#orders/getorder/t=response&c=200&path=initial_payment_method/type
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
        // Methods introduced more recently than 2023 (added to the end, to avoid number mismatches in the backing type)
        APPLE_PAY_CARD,
        GOOGLE_PAY_CARD,
        CARTES_BANCAIRES,
        BLIK,
        TWINT,
        BANCONTACT
    }
}
