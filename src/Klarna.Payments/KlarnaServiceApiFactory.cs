using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using EPiServer.Commerce.Order;
using Klarna.Common;
using Klarna.Common.Extensions;
using Klarna.Rest.Transport;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Refit;

namespace Klarna.Payments
{
    /// <summary>
    /// Factory methods to create an instance of IKlarnaServiceApi
    /// Initializes it for a specific payment method and a specific market (as the API settings can vary)
    /// </summary>
    public class KlarnaServiceApiFactory
    {
        public virtual IKlarnaServiceApi Create(IPayment payment, IMarket market)
        {
            return Create(PaymentManager.GetPaymentMethod(payment.PaymentMethodId), market.MarketId);
        }

        public virtual IKlarnaServiceApi Create(PaymentMethodDto paymentMethodDto, MarketId marketMarketId)
        {
            return Create(paymentMethodDto.GetConnectionConfiguration(marketMarketId));
        }

        public virtual IKlarnaServiceApi Create(ConnectionConfiguration connectionConfiguration)
        {
            var byteArray = Encoding.ASCII.GetBytes($"{connectionConfiguration.Username}:{connectionConfiguration.Password}");
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(connectionConfiguration.ApiUrl)
            };
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Platform", $"EPiServer_{typeof(EPiServer.Core.IContent).Assembly.GetName().Version.ToString()}"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Module", $"Klarna.Payments_{typeof(Klarna.Payments.KlarnaPaymentsService).Assembly.GetName().Version.ToString()}"));

            return RestService.For<IKlarnaServiceApi>(httpClient);
        }
    }
}