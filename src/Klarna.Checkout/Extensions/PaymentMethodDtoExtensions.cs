using Klarna.Common.Extensions;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Newtonsoft.Json;

namespace Klarna.Checkout.Extensions
{
    public static class PaymentMethodDtoExtensions
    {
        public static CheckoutConfiguration GetKlarnaCheckoutConfiguration(
            this PaymentMethodDto paymentMethodDto, MarketId marketId)
        {
            var parameter = paymentMethodDto.GetParameter(
                $"{marketId.Value}_{Common.Constants.KlarnaSerializedMarketOptions}", string.Empty);

            var configuration = JsonConvert.DeserializeObject<CheckoutConfiguration>(parameter);

            if (configuration == null)
            {
                return new CheckoutConfiguration();
            }
            return configuration;
        }
    }
}
