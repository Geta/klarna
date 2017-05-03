using Mediachase.Commerce.Orders.Dto;

namespace Klarna.Common
{
    public abstract class ConnectionFactory
    {
        public abstract ConnectionConfiguration GetConnectionConfiguration(PaymentMethodDto paymentMethod);
    }
}
