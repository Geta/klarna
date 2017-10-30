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
            context.Services.Intercept<IOrderNumberGenerator>((locator, defaultService) =>
            {
                return new OrderNumberGeneratorDecorator(defaultService);
            });
        }

        public void Uninitialize(InitializationEngine context)
        {

        }
    }
}
