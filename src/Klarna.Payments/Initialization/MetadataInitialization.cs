using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using Klarna.Common.Initialization;
using Mediachase.Commerce.Catalog;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;

namespace Klarna.Payments.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    internal class MetadataInitialization : MetadataInitializationBase, IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            MetaDataContext mdContext = CatalogContext.MetaDataContext;

            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaSessionIdCartField), Common.Constants.CartClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaClientTokenCartField), Common.Constants.CartClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaPaymentsDescriptorCartField), Common.Constants.CartClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaPaymentMethodCategoriesCartField), Common.Constants.CartClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaAllowSharingOfPersonalInformationCartField, MetaDataType.Boolean), Common.Constants.CartClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.CartOrderNumberTempCartField), Common.Constants.CartClass);

            // Other payment meta fields
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaConfirmationUrlPaymentField), Common.Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.AuthorizationTokenPaymentField), Common.Constants.OtherPaymentClass);
        }

        public void Uninitialize(InitializationEngine context)
        {

        }

        protected override string IntegrationName => "Klarna Payments";
    }
}
