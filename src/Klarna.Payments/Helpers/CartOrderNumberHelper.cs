using System;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;

namespace Klarna.Payments.Helpers
{
    public class CartOrderNumberHelper
    {
        public static string GenerateOrderNumber(IOrderGroup orderGroup)
        {
            var orderNumberField = orderGroup.Properties[Constants.CartOrderNumberTempCartField];
            var orderNumber = string.Empty;

            if (string.IsNullOrEmpty(orderNumberField?.ToString()))
            {
                if (orderGroup is Cart)
                {
                    var cart = (Cart) orderGroup;
                    orderNumber = cart.GenerateOrderNumber(cart);
                }
                orderGroup.Properties[Constants.CartOrderNumberTempCartField] = orderNumber;
                return orderNumber;
            }
            return orderNumberField.ToString();
        }

        public static string GetOrderNumber(OrderGroup orderGroup)
        {
            return orderGroup.GetString(Constants.CartOrderNumberTempCartField);
        }
    }
}
