using System;
using System.Linq;
using EPiServer.Globalization;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Newtonsoft.Json;

namespace Klarna.Common.Extensions
{
    public static class PaymentMethodDtoExtensions
    {
        public static ConnectionConfiguration GetConnectionConfiguration(this PaymentMethodDto paymentMethodDto,
            MarketId marketId)
        {
            var configurations = JsonConvert.DeserializeObject<ConnectionConfiguration[]>(paymentMethodDto.GetParameter(Constants.KlarnaSerializedMarketOptions, "[]"));
            var config = configurations.FirstOrDefault(x => x.MarketId.Equals(marketId.ToString()));

            if (config == null)
            {
                throw new Exception(
                    $"PaymentMethod {paymentMethodDto.PaymentMethod.FirstOrDefault()?.SystemKeyword} is not configured for market {marketId} and language {ContentLanguage.PreferredCulture.Name}");
            }

            return new ConnectionConfiguration
            {
                ApiUrl = config.ApiUrl,
                Username = config.Username,
                Password = config.Password
            };
        }
    }
}
