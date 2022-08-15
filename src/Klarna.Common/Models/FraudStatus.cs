namespace Klarna.Common.Models
{
    public enum FraudStatus
    {
        ACCEPTED,
        PENDING,
        REJECTED
    }

    public enum NotificationFraudStatus
    {
        FRAUD_RISK_ACCEPTED,
        FRAUD_RISK_REJECTED,
        FRAUD_RISK_STOPPED
    }
}
