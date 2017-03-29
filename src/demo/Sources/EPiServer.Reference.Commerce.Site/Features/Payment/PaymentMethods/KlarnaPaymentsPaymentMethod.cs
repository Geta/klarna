using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders;
using System;
using System.ComponentModel;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    public class KlarnaPaymentsPaymentMethod : PaymentMethodBase, IDataErrorInfo
    {
        static readonly string[] ValidatedProperties = 
        {
            "CreditCardNumber",
            "CreditCardSecurityCode",
            "ExpirationYear",
            "ExpirationMonth",
        };

        public KlarnaPaymentsPaymentMethod()
            : this(LocalizationService.Current, ServiceLocator.Current.GetInstance<IOrderGroupFactory>())
        {
        }

        public KlarnaPaymentsPaymentMethod(LocalizationService localizationService, IOrderGroupFactory orderGroupFactory)
            : base(localizationService, orderGroupFactory)
        {
        }
        

        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = orderGroup.CreateCardPayment(_orderGroupFactory);
            payment.CardType = "Credit card";
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = "GenericCreditCard";
            payment.Amount = amount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = TransactionType.Authorization.ToString();
            return payment;
        }

        public override void PostProcess(IPayment payment)
        {
            var creditCardPayment = (ICreditCardPayment)payment;
            var visibleDigits = 4;
            var cardNumberLength = creditCardPayment.CreditCardNumber.Length;
            creditCardPayment.CreditCardNumber = new string('*', cardNumberLength - visibleDigits) 
                + creditCardPayment.CreditCardNumber.Substring(cardNumberLength - visibleDigits, visibleDigits);
        }

        public override bool ValidateData()
        {
            return true;
        }

        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }

        public string Error { get; }
    }
}