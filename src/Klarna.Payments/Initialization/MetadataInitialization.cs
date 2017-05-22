using System;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using Mediachase.Commerce.Catalog;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;
using MetaClass = Mediachase.MetaDataPlus.Configurator.MetaClass;
using MetaField = Mediachase.MetaDataPlus.Configurator.MetaField;

namespace Klarna.Payments.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    internal class MetadataInitialization : IInitializableModule
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(MetadataInitialization));

        public void Initialize(InitializationEngine context)
        {
            MetaDataContext mdContext = CatalogContext.MetaDataContext;
            
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaSessionIdField), Common.Constants.CartClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaClientTokenField), Common.Constants.CartClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaAllowSharingOfPersonalInformationField, MetaDataType.Boolean), Common.Constants.CartClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.CartOrderNumberTempField), Common.Constants.CartClass);
            
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.ConfirmationUrlField), Common.Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.NotificationUrlField), Common.Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.SendProductAndImageUrlField), Common.Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.UseAttachmentsField), Common.Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaConfirmationUrlField), Common.Constants.OtherPaymentClass);
            
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.AuthorizationTokenPaymentMethodField), Common.Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Common.Constants.KlarnaOrderIdField), Common.Constants.OrderNamespace);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Common.Constants.KlarnaOrderIdField), Common.Constants.PurchaseOrderClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Common.Constants.FraudStatusPaymentMethodField), Common.Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.PreAssesmentCountriesField), Common.Constants.OtherPaymentClass);
        }

        public void Uninitialize(InitializationEngine context)
        {

        }

        private MetaField GetOrCreateCardField(MetaDataContext mdContext, string fieldName, MetaDataType metaDataType = MetaDataType.LongString)
        {

            var f = MetaField.Load(mdContext, fieldName);
            if (f == null)
            {
                Logger.Debug($"Adding meta field '{fieldName}' for Klarna payments integration.");
                f = MetaField.Create(mdContext, Common.Constants.OrderNamespace, fieldName, fieldName, string.Empty, metaDataType, Int32.MaxValue, true, false, false, false);
            }
            return f;
        }
        
        private void JoinField(MetaDataContext mdContext, MetaField field, string metaClassName)
        {
            var cls = MetaClass.Load(mdContext, metaClassName);

            if (MetaFieldIsNotConnected(field, cls))
            {
                cls.AddField(field);
            }
        }
        
        private static bool MetaFieldIsNotConnected(MetaField field, MetaClass cls)
        {
            return cls != null && !cls.MetaFields.Contains(field);
        }
    }
}
