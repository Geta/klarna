using EPiServer.Commerce.Order;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace Klarna.Payments.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    [ModuleDependency(typeof(CommerceInitialization))]
    internal class PaymentsInitialization : IConfigurableModule
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
            context.Services.Intercept<IOrderNumberGenerator>((locator, defaultService) => new OrderNumberGeneratorDecorator(defaultService));
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
