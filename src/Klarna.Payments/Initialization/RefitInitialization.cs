using System;
using System.Net.Http;
using System.Text;
using EPiServer.Commerce.Order;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Klarna.Common;
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
        private Injected<ConnectionFactory> _connectionFactory;

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
            var conn = GetConnectionConfiguration();

            var byteArray = Encoding.ASCII.GetBytes($"{conn.Username}:{conn.Password}");
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(conn.ApiUrl)
            };
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var refitSettings = new RefitSettings();

            return RestService.For<IKlarnaServiceApi>(httpClient, refitSettings);
        }

        private ConnectionConfiguration GetConnectionConfiguration()
        {
            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaPaymentSystemKeyword, ContentLanguage.PreferredCulture.Name);
            if (paymentMethod != null)
            {
                return _connectionFactory.Service.GetConnectionConfiguration(paymentMethod);
            }
            return null;
        }
    }
}
