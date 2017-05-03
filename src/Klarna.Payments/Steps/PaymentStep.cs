using System;
using System.Net;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Klarna.Common;
using Klarna.OrderManagement;
using Klarna.Payments.Extensions;
using Klarna.Rest.Transport;
using Mediachase.Commerce.Orders.Managers;

namespace Klarna.Payments.Steps
{
    public abstract class PaymentStep
    {
        protected Injected<IKlarnaService> KlarnaService;
        protected IKlarnaOrderService KlarnaOrderService;
        protected Injected<ConnectionFactory> ConnectionFactory;
        protected PaymentStep Successor;

        protected PaymentStep(IPayment payment)
        {
            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaPaymentSystemKeyword, ContentLanguage.PreferredCulture.Name);
            if (paymentMethod != null)
            {
                KlarnaOrderService = new KlarnaOrderService(ConnectionFactory.Service.GetConnectionConfiguration(paymentMethod));
            }
        }

        public void SetSuccessor(PaymentStep successor)
        {
            this.Successor = successor;
        }

        public abstract bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, ref string message);
        
        protected void AddNoteAndSaveChanges(IOrderGroup orderGroup, string transactionType, string noteMessage)
        {
            var noteTitle = $"Klarna payment {transactionType.ToLower()}";

            orderGroup.AddNote(noteTitle, $"Payment {transactionType.ToLower()}: {noteMessage}");
        }

        protected string GetExceptionMessage(Exception ex)
        {
            var exceptionMessage = string.Empty;
            switch (ex)
            {
                case ApiException apiException:
                    exceptionMessage =
                        $"{apiException.ErrorMessage.CorrelationId} " +
                        $"{apiException.ErrorMessage.ErrorCode} " +
                        $"{apiException.ErrorMessage.ErrorMessages}";
                    break;
                case WebException webException:
                    exceptionMessage = webException.Message;
                    break;
            }
            return exceptionMessage;
        }
    }
}
