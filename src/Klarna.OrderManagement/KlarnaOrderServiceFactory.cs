using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using EPiServer.Commerce.Order;
using Klarna.Common;
using Klarna.Common.Extensions;
using Klarna.Rest;
using Klarna.Rest.Transport;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Refit;

namespace Klarna.OrderManagement
{
    /// <summary>
    /// Factory methods to create an instance of IKlarnaOrderService
    /// Initializes it for a specific payment method and a specific market (as the API settings can vary)
    /// </summary>
    public class KlarnaOrderServiceFactory
    {
        public virtual IKlarnaOrderService Create(IPayment payment, IMarket market)
        {
            return Create(PaymentManager.GetPaymentMethod(payment.PaymentMethodId), market.MarketId);
        }

        public virtual IKlarnaOrderService Create(PaymentMethodDto paymentMethodDto, MarketId marketMarketId)
        {
            return Create(paymentMethodDto.GetConnectionConfiguration(marketMarketId));
        }

        public virtual IKlarnaOrderService Create(ConnectionConfiguration connectionConfiguration)
        {
            var client = new Client(ConnectorFactory.Create(connectionConfiguration.Username, connectionConfiguration.Password, new Uri(connectionConfiguration.ApiUrl)));

            var byteArray = Encoding.ASCII.GetBytes($"{connectionConfiguration.Username}:{connectionConfiguration.Password}");
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(connectionConfiguration.ApiUrl)
            };
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Platform", $"EPiServer_{typeof(EPiServer.Core.IContent).Assembly.GetName().Version.ToString()}"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Module", $"Klarna.OrderManagement_{typeof(Klarna.OrderManagement.KlarnaOrderService).Assembly.GetName().Version.ToString()}"));

            return new KlarnaOrderService(client, RestService.For<IKlarnaOrderServiceApi>(httpClient));
        }
    }
}