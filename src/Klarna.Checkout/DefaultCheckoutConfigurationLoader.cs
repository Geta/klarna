using System;
using EPiServer.ServiceLocation;
using Klarna.Common;
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
        private ILanguageService _languageService;

        public DefaultCheckoutConfigurationLoader(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        public CheckoutConfiguration GetConfiguration(MarketId marketId, string languageId)
        {
            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(
                Constants.KlarnaCheckoutSystemKeyword, languageId, returnInactive: true);
            if (paymentMethod == null)
            {
                throw new Exception(
                    $"PaymentMethod {Constants.KlarnaCheckoutSystemKeyword} is not configured for market {marketId} and language {languageId}");
            }
            return GetKlarnaCheckoutConfiguration(paymentMethod, marketId);
        }

        public CheckoutConfiguration GetConfiguration(MarketId marketId)
        {
            return GetConfiguration(marketId, _languageService.GetPreferredCulture().Name);
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