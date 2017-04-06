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
            return _inner.GenerateOrderNumber(orderGroup);
        }
    }
}
