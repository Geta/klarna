using System;
using EPiServer.Commerce.Order;
using Klarna.Common;
using Klarna.Common.Extensions;
using Klarna.Rest;
using Klarna.Rest.Transport;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace Klarna.OrderManagement
{
    /// <summary>
    /// Factory methods to create an instance of IKlarnaOrderService
    /// Initializes it for a specific payment method and a specific market (as the API settings can vary)
    /// </summary>
    public class KlarnaOrderServiceFactory
    {
        public static IKlarnaOrderService Create(IPayment payment, IMarket market)
        {
            return Create(PaymentManager.GetPaymentMethod(payment.PaymentMethodId), market.MarketId);
        }

        public static IKlarnaOrderService Create(PaymentMethodDto paymentMethodDto, MarketId marketMarketId)
        {
            return Create(paymentMethodDto.GetConnectionConfiguration(marketMarketId));
        }

        public static IKlarnaOrderService Create(ConnectionConfiguration connectionConfiguration)
        {
            var client = new Client(ConnectorFactory.Create(connectionConfiguration.Username, connectionConfiguration.Password, new Uri(connectionConfiguration.ApiUrl)));
            return new KlarnaOrderService(client);
        }
    }
}