using System;
using System.Linq;
using EPiServer.Globalization;
using Klarna.Common.Extensions;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Newtonsoft.Json;

namespace Klarna.Checkout.Extensions
{
    public static class PaymentMethodDtoExtensions
    {
        public static Configuration GetConfiguration(this PaymentMethodDto paymentMethodDto,
            MarketId marketId)
        {
            var configuration = JsonConvert.DeserializeObject<Configuration>(paymentMethodDto.GetParameter($"{marketId.Value}_{Common.Constants.KlarnaSerializedMarketOptions}", string.Empty));

            if (configuration == null)
            {
                return new Configuration();
                throw new Exception(
                    $"PaymentMethod {Constants.KlarnaCheckoutSystemKeyword} is not configured for market {marketId} and language {ContentLanguage.PreferredCulture.Name}");
            }
            return configuration; 

            /*var allConfigurations = JsonConvert.DeserializeObject<Configuration[]>(paymentMethodDto.GetParameter(Common.Constants.KlarnaSerializedMarketOptions, "[]"));
            var configurationForMarket = allConfigurations.FirstOrDefault(x => x.MarketId.Equals(marketId.ToString(), StringComparison.InvariantCultureIgnoreCase));
            if (configurationForMarket == null)
            {
                return new Configuration();
                throw new Exception(
                    $"PaymentMethod {Constants.KlarnaCheckoutSystemKeyword} is not configured for market {marketId} and language {ContentLanguage.PreferredCulture.Name}");
            }
            return configurationForMarket;*/
        }
    }
}
