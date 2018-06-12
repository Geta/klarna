using EPiServer.Commerce.Order;

namespace Klarna.Payments.Extensions
{
    public static class CartExtensions
    {
        public static string GetKlarnaSessionId(this ICart cart)
        {
            return cart.Properties[Constants.KlarnaSessionIdCartField]?.ToString();
        }

        public static string GetKlarnaClientToken(this ICart cart)
        {
            return cart.Properties[Constants.KlarnaClientTokenCartField]?.ToString();
        }
    }
}