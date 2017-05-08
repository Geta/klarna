using Mediachase.Commerce.Orders.Dto;

namespace Klarna.Common
{
    public interface IConnectionFactory
    {
        ConnectionConfiguration GetConnectionConfiguration(PaymentMethodDto paymentMethod);
    }
}
