using EPiServer.Commerce.Order;
using Klarna.Common;
using Klarna.Common.Configuration;
using Klarna.Common.Models;
using Mediachase.Commerce.Markets;

namespace Klarna.OrderManagement
{
    /// <summary>
    /// Factory methods to create an instance of IKlarnaOrderService
    /// Initializes it for a specific payment method and a specific market (as the API settings can vary)
    /// </summary>
    public class KlarnaOrderServiceFactory : IKlarnaOrderServiceFactory
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IMarketService _marketService;
        private readonly IConfigurationLoader _configurationLoader;

        public KlarnaOrderServiceFactory(IOrderRepository orderRepository, IPaymentProcessor paymentProcessor, IOrderGroupCalculator orderGroupCalculator,
            IMarketService marketService, IConfigurationLoader configurationLoader)
        {
            _orderRepository = orderRepository;
            _paymentProcessor = paymentProcessor;
            _orderGroupCalculator = orderGroupCalculator;
            _marketService = marketService;
            _configurationLoader = configurationLoader;
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
            
            return new KlarnaOrderService(client, _orderRepository, _paymentProcessor, _orderGroupCalculator, _marketService, _configurationLoader);
        }
    }
}