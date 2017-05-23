using System;
using EPiServer.Globalization;
using Klarna.Common.Extensions;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Newtonsoft.Json;

namespace Klarna.Payments.Extensions
{
    public static class PaymentMethodDtoExtensions
    {
        public static PaymentsConfiguration GetKlarnaPaymentsConfiguration(this PaymentMethodDto paymentMethodDto,
            MarketId marketId)
        {
            var configuration = JsonConvert.DeserializeObject<PaymentsConfiguration>(paymentMethodDto.GetParameter($"{marketId.Value}_{Common.Constants.KlarnaSerializedMarketOptions}", string.Empty));
            
            if (configuration == null)
            {
                return new PaymentsConfiguration();
                throw new Exception(
                    $"PaymentMethod {Constants.KlarnaPaymentSystemKeyword} is not configured for market {marketId} and language {ContentLanguage.PreferredCulture.Name}");
            }
            return configuration;
        }
    }
}
