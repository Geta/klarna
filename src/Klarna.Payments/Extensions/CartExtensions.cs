using System;
using System.Collections.Generic;
using EPiServer.Commerce.Order;

namespace Klarna.Payments.Extensions
{
    public static class CartExtensions
    {
        public static string GetKlarnaSessionId(this ICart cart)
        {
            return cart.Properties[Constants.KlarnaSessionIdCartField]?.ToString();
        }

        public static void SetKlarnaSessionId(this ICart cart, string sessionId)
        {
            cart.Properties[Constants.KlarnaSessionIdCartField] = sessionId;
        }

        public static string GetKlarnaClientToken(this ICart cart)
        {
            return cart.Properties[Constants.KlarnaClientTokenCartField]?.ToString();
        }

        public static void SetKlarnaClientToken(this ICart cart, string clientToken)
        {
            cart.Properties[Constants.KlarnaClientTokenCartField] = clientToken;
        }
    }
}