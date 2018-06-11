using Mediachase.Commerce;

namespace Klarna.Checkout
{
    public interface ICheckoutConfigurationLoader
    {
        CheckoutConfiguration GetConfiguration(MarketId marketId);
    }
}