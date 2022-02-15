using EPiServer.ServiceLocation;
using Klarna.Common;
using Klarna.Common.Configuration;
using Klarna.Common.Models;

namespace Klarna.OrderManagement
{
    /// <summary>
    /// Factory methods to create an instance of IKlarnaOrderService
    /// Initializes it for a specific payment method and a specific market (as the API settings can vary)
    /// </summary>
    [ServiceConfiguration(typeof(IKlarnaOrderServiceFactory))]
    public class KlarnaOrderServiceFactory : IKlarnaOrderServiceFactory
    {
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