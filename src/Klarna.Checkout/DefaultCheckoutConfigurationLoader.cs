using System;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Klarna.Common.Extensions;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Newtonsoft.Json;

namespace Klarna.Checkout
{
    [ServiceConfiguration(typeof(ICheckoutConfigurationLoader))]
    public class DefaultCheckoutConfigurationLoader : ICheckoutConfigurationLoader
    {
        public CheckoutConfiguration GetConfiguration(MarketId marketId)
        {
            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(
                Constants.KlarnaCheckoutSystemKeyword, ContentLanguage.PreferredCulture.Name, returnInactive: true);
            if (paymentMethod == null)
            {
                throw new Exception(
                    $"PaymentMethod {Constants.KlarnaCheckoutSystemKeyword} is not configured for market {marketId} and language {ContentLanguage.PreferredCulture.Name}");
            }
            return GetKlarnaCheckoutConfiguration(paymentMethod, marketId);
        }

        private static CheckoutConfiguration GetKlarnaCheckoutConfiguration(
            PaymentMethodDto paymentMethodDto, MarketId marketId)
        {
            var parameter = paymentMethodDto.GetParameter(
                $"{marketId.Value}_{Common.Constants.KlarnaSerializedMarketOptions}", string.Empty);

            var configuration = JsonConvert.DeserializeObject<CheckoutConfiguration>(parameter);

            return configuration ?? new CheckoutConfiguration();
        }
    }
}