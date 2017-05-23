using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using Klarna.Common.Initialization;
using Mediachase.Commerce.Catalog;
using Mediachase.MetaDataPlus;

namespace Klarna.OrderManagement.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    internal class MetadataInitialization : MetadataInitializationBase, IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            MetaDataContext mdContext = CatalogContext.MetaDataContext;
            
            // Purchase order meta fields
            JoinField(mdContext, GetOrCreateCardField(mdContext, Common.Constants.KlarnaOrderIdField), Common.Constants.PurchaseOrderClass);

            // Other payment meta fields
            JoinField(mdContext, GetOrCreateCardField(mdContext, Common.Constants.FraudStatusPaymentField), Common.Constants.OtherPaymentClass);
        }

        public void Uninitialize(InitializationEngine context)
        {

        }

        protected override string IntegrationName => "Order Management";
    }
}
