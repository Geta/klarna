using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders;
using System;
using System.ComponentModel;
using Klarna.Checkout;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    public class KlarnaCheckoutPaymentMethod : PaymentMethodBase, IDataErrorInfo
    {
        public KlarnaCheckoutPaymentMethod()
            : this(LocalizationService.Current, ServiceLocator.Current.GetInstance<IOrderGroupFactory>())
        {
        }

        public KlarnaCheckoutPaymentMethod(LocalizationService localizationService, IOrderGroupFactory orderGroupFactory)
            : base(localizationService, orderGroupFactory)
        {
        }
        

        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = orderGroup.CreatePayment(_orderGroupFactory);
            payment.PaymentType = PaymentType.Other;
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = Constants.KlarnaCheckoutSystemKeyword;
            payment.Amount = amount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = TransactionType.Authorization.ToString();
            return payment;
        }

        public override void PostProcess(IPayment payment)
        {
           
        }

        public override bool ValidateData()
        {
            return true;
        }

        public string this[string columnName]
        {
            get { return string.Empty; }
        }

        public string Error { get; }
    }
}