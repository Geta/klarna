using System;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;
using MetaClass = Mediachase.MetaDataPlus.Configurator.MetaClass;
using MetaField = Mediachase.MetaDataPlus.Configurator.MetaField;

namespace Klarna.Payments.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    internal class OrderInitialization : IInitializableModule
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(OrderInitialization));

        public void Initialize(InitializationEngine context)
        {
            OrderContext.Current.OrderGroupUpdated += Current_OrderGroupUpdated;
        }

        private void Current_OrderGroupUpdated(object sender, OrderGroupEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Uninitialize(InitializationEngine context)
        {

        }
    }
}
