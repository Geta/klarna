using System;
using System.Net.Http;
using System.Text;
using EPiServer.Commerce.Order;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Klarna.Common;
using Klarna.Common.Extensions;
using Mediachase.Commerce;
using Mediachase.Commerce.Initialization;
using Mediachase.Commerce.Orders.Managers;
using Refit;

namespace Klarna.Payments.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    [ModuleDependency(typeof(CommerceInitialization))]
    internal class RefitInitialization : IConfigurableModule
    {
        private static bool _initialized;

        public void Initialize(InitializationEngine context)
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Container.Configure(x => x.For<IKlarnaServiceApi>().Use(() => GetInstance()));
            
            context.Services.Intercept<IOrderNumberGenerator>((locator, defaultService) =>
            {
                return new OrderNumberGeneratorDecorator(defaultService);
            });
        }

        public void Uninitialize(InitializationEngine context)
        {

        }

        public IKlarnaServiceApi GetInstance()
        {
            var market = ServiceLocator.Current.GetInstance<ICurrentMarket>();
            var conn = GetConnectionConfiguration(market.GetCurrentMarket().MarketId);

            var byteArray = Encoding.ASCII.GetBytes($"{conn.Username}:{conn.Password}");
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(conn.ApiUrl)
            };
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var refitSettings = new RefitSettings();

            return RestService.For<IKlarnaServiceApi>(httpClient, refitSettings);
        }

        private ConnectionConfiguration GetConnectionConfiguration(MarketId marketId)
        {
            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaPaymentSystemKeyword, ContentLanguage.PreferredCulture.Name);
            if (paymentMethod != null)
            {
                return paymentMethod.GetConnectionConfiguration(marketId);
            }
            return null;
        }
    }
}
