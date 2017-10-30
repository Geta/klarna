using EPiServer.Commerce.Order;

namespace Klarna.Common.Extensions
{
    public static class PaymentExtensions
    {
        public static bool IsKlarnaPayment(this IPayment payment)
        {
            return payment?.PaymentMethodName?.StartsWith(Constants.KlarnaSystemKeyword) ?? false;
        }
    }
}