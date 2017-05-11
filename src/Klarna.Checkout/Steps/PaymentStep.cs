using System;
using System.Net;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Klarna.Common;
using Klarna.Common.Extensions;
using Klarna.OrderManagement;
using Klarna.Rest.Transport;
using Mediachase.Commerce.Orders.Managers;

namespace Klarna.Checkout.Steps
{
    public abstract class PaymentStep
    {
        protected Injected<IKlarnaCheckoutService> KlarnaService;
        protected IKlarnaOrderService KlarnaOrderService;
        protected Injected<IConnectionFactory> ConnectionFactory;
        protected PaymentStep Successor;

        protected PaymentStep(IPayment payment)
        {
            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaCheckoutSystemKeyword, ContentLanguage.PreferredCulture.Name);
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
            var noteTitle = $"Klarna checkout {transactionType.ToLower()}";

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
                        $"{string.Join(", ", apiException.ErrorMessage.ErrorMessages)}";
                    break;
                case WebException webException:
                    exceptionMessage = webException.Message;
                    break;
            }
            return exceptionMessage;
        }
    }
}
