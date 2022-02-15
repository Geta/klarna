using EPiServer.Commerce.Order;
using Mediachase.Commerce;

namespace Klarna.Common.Configuration
{
    public interface IConfigurationLoader
    {
        ConnectionConfiguration GetConfiguration(IPayment payment, MarketId marketId);
        CheckoutConfiguration GetCheckoutConfiguration(MarketId marketId);
        PaymentsConfiguration GetPaymentsConfiguration(MarketId marketId);
    }
}