using EPiServer.Commerce.Order;
using Klarna.Common.Models;

namespace Klarna.Common.Extensions
{
    public static class PaymentExtensions
    {
        public static bool IsKlarnaPayment(this IPayment payment)
        {
            return payment?.PaymentMethodName?.StartsWith(Constants.KlarnaSystemKeyword) ?? false;
        }

        public static bool HasFraudStatus(this IPayment payment, NotificationFraudStatus fraudStatus)
        {
            return payment.Properties[Constants.FraudStatusPaymentField]?.ToString() == fraudStatus.ToString();
        }

        public static bool HasFraudStatus(this IPayment payment, FraudStatus fraudStatus)
        {
            return payment.Properties[Constants.FraudStatusPaymentField]?.ToString() == fraudStatus.ToString();
        }
    }
}