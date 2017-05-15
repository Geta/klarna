using System;
using EPiServer.Commerce.Order;

namespace Klarna.Payments
{
    public class OrderNumberGeneratorDecorator : IOrderNumberGenerator
    {
        private readonly IOrderNumberGenerator _inner;

        public OrderNumberGeneratorDecorator(IOrderNumberGenerator inner)
        {
            _inner = inner;
        }

        public string GenerateOrderNumber(IOrderGroup orderGroup)
        {
            var orderNumberField = orderGroup.Properties[Constants.CartOrderNumberTempField];
            
            if (string.IsNullOrEmpty(orderNumberField?.ToString()))
            {
                var orderNumber = _inner.GenerateOrderNumber(orderGroup);

                orderGroup.Properties[Constants.CartOrderNumberTempField] = orderNumber;

                return orderNumber;
            }
            return orderNumberField.ToString();
        }
    }
}
