using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Klarna.OrderManagement;
using Klarna.OrderManagement.Refunds;

namespace EPiServer.Reference.Commerce.Manager.OrderManagement
{
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    public class SiteInitialization : IConfigurableModule
    {
        public void Initialize(InitializationEngine context)
        {
          
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            var services = context.Services;

            services.AddTransient<RefundBuilder, DemoRefundBuilder>();
            services.AddTransient<CaptureBuilder, DemoCaptureBuilder>();
        }

        public void Uninitialize(InitializationEngine context) { }
    }
}