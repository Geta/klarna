using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
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
    internal class MetadataInitialization : IInitializableModule
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(MetadataInitialization));

        public void Initialize(InitializationEngine context)
        {
            MetaDataContext mdContext = CatalogContext.MetaDataContext;

            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaUsernameField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaPasswordField), Constants.OtherPaymentClass);

            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaSessionIdField), Constants.CartClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaClientTokenField), Constants.CartClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.CartOrderNumberTempField), Constants.CartClass);

            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaLogoUrlField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaWidgetColorDetailsField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaWidgetColorButtonField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaWidgetColorButtonTextField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaWidgetColorCheckboxField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaWidgetColorCheckboxCheckmarkField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaWidgetColorHeaderField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaWidgetColorLinkField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaWidgetColorBorderField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaWidgetColorBorderSelectedField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaWidgetColorTextField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaWidgetColorTextSecondaryField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaWidgetRadiusBorderField), Constants.OtherPaymentClass);

            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.ConfirmationUrlField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.NotificationUrlField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.SendProductAndImageUrlField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.UseAttachmentsField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaConfirmationUrlField), Constants.OtherPaymentClass);
            
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.AuthorizationTokenPaymentMethodField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaOrderIdField), Constants.OrderNamespace);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.KlarnaOrderIdField), Constants.PurchaseOrderClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.FraudStatusPaymentMethodField), Constants.OtherPaymentClass);
            JoinField(mdContext, GetOrCreateCardField(mdContext, Constants.PreAssesmentCountriesField), Constants.OtherPaymentClass);
        }

        public void Uninitialize(InitializationEngine context)
        {

        }

        private MetaField GetOrCreateCardField(MetaDataContext mdContext, string fieldName)
        {

            var f = MetaField.Load(mdContext, fieldName);
            if (f == null)
            {
                Logger.Debug($"Adding meta field '{fieldName}' for Klarna payments integration.");
                f = MetaField.Create(mdContext, Constants.OrderNamespace, fieldName, fieldName, string.Empty, MetaDataType.LongString, Int32.MaxValue, true, false, false, false);
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
