using System;
using System.Net.Http;
using System.Text;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Refit;

namespace Klarna.Payments.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class RefitInitialization : IConfigurableModule
    {
        private static bool _initialized;

        // TODO: make configurable
        private string _apiUrl = "https://api.playground.klarna.com/";
        private string _username = "username";
        private string  _password = "password1234";

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
            var byteArray = Encoding.ASCII.GetBytes($"{_username}:{_password}");
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_apiUrl)
            };
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var refitSettings = new RefitSettings();

            var restService = RestService.For<IKlarnaServiceApi>(httpClient, refitSettings);
            context.Container.Configure(x => x.For<IKlarnaServiceApi>()
                .Singleton()
                .Add(() => restService));
        }

        public void Uninitialize(InitializationEngine context)
        {

        }
    }
}
