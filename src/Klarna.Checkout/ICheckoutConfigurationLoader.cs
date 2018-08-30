using Mediachase.Commerce;

namespace Klarna.Checkout
{
    public interface ICheckoutConfigurationLoader
    {
        CheckoutConfiguration GetConfiguration(MarketId marketId);

        CheckoutConfiguration GetConfiguration(MarketId marketId, string languageId);
    }
}