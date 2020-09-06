using EPiServer.Commerce.Order;
using Klarna.Common;
using Klarna.Common.Extensions;
using Klarna.Common.Models;
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
            string userAgent = $"Platform/Episerver.Commerce_{typeof(EPiServer.Commerce.ApplicationContext).Assembly.GetName().Version} Module/Klarna.OrderManagement_{typeof(KlarnaOrderService).Assembly.GetName().Version}";
                
            var client =  new OrderManagementStore(new ApiSession
            {
                ApiUrl = connectionConfiguration.ApiUrl,
                UserAgent = userAgent,
                Credentials = new ApiCredentials
                {
                    Username = connectionConfiguration.Username,
                    Password = connectionConfiguration.Password
                }
            }, new JsonSerializer());
            
            return new KlarnaOrderService(client);
        }
    }
}