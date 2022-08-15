using System;
using System.Linq;
using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Managers;
using Microsoft.Extensions.Options;

namespace Klarna.Common.Configuration
{
    public class DefaultConfigurationLoader : IConfigurationLoader
    {
        private readonly IOptionsSnapshot<CheckoutConfiguration> _checkoutOptionsAccessor;
        private readonly IOptionsSnapshot<PaymentsConfiguration> _paymentsOptionsAccessor;

        public DefaultConfigurationLoader(IOptionsSnapshot<CheckoutConfiguration> checkoutOptionsAccessor, IOptionsSnapshot<PaymentsConfiguration> paymentsOptionsAccessor)
        {
            _checkoutOptionsAccessor = checkoutOptionsAccessor;
            _paymentsOptionsAccessor = paymentsOptionsAccessor;
        }

        public ConnectionConfiguration GetConfiguration(IPayment payment, MarketId marketId)
        {
            var configuration = new ConnectionConfiguration();

            if (payment == null)
            {
                return configuration;
            }

            var paymentMethod = PaymentManager.GetPaymentMethod(payment.PaymentMethodId)?.PaymentMethod.FirstOrDefault();

            if (paymentMethod == null)
            {
                return configuration;
            }

            if (paymentMethod.SystemKeyword.Equals("KlarnaCheckout", StringComparison.InvariantCultureIgnoreCase))
            {
                var checkoutConfig = GetCheckoutConfiguration(marketId);

                configuration.MarketId = marketId.Value;
                configuration.ApiUrl = checkoutConfig.ApiUrl;
                configuration.Username = checkoutConfig.Username;
                configuration.Password = checkoutConfig.Password;
            }

            if (paymentMethod.SystemKeyword.Equals("KlarnaPayments", StringComparison.InvariantCultureIgnoreCase))
            {
                var paymentsConfig = GetPaymentsConfiguration(marketId);

                configuration.MarketId = marketId.Value;
                configuration.ApiUrl = paymentsConfig.ApiUrl;
                configuration.Username = paymentsConfig.Username;
                configuration.Password = paymentsConfig.Password;
            }

            return configuration;
        }

        public CheckoutConfiguration GetCheckoutConfiguration(MarketId marketId)
        {
            var configuration = _checkoutOptionsAccessor.Get(marketId.Value);

            if (configuration != null)
            {
                configuration.MarketId = marketId.Value;

                return configuration;
            }

            return new CheckoutConfiguration();
        }

        public PaymentsConfiguration GetPaymentsConfiguration(MarketId marketId)
        {
            var configuration = _paymentsOptionsAccessor.Get(marketId.Value);

            if (configuration != null)
            {
                configuration.MarketId = marketId.Value;

                return configuration;
            }

            return new PaymentsConfiguration();
        }
    }
}