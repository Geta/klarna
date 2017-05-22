using System;
using System.Linq;
using System.Net;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Klarna.Common;
using Klarna.Common.Extensions;
using Klarna.Rest.Transport;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace Klarna.OrderManagement.Steps
{
    public abstract class PaymentStep
    {
        protected PaymentStep Successor;

        protected PaymentMethodDto PaymentMethod { get; set; }
        public MarketId MarketId { get; }

        protected IKlarnaOrderService KlarnaOrderService;

        protected PaymentStep(IPayment payment, MarketId marketId)
        {
            MarketId = marketId;

            PaymentMethod = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
            if (PaymentMethod != null)
            {
                KlarnaOrderService = new KlarnaOrderService(PaymentMethod.GetConnectionConfiguration(marketId));
            }
        }

        public void SetSuccessor(PaymentStep successor)
        {
            this.Successor = successor;
        }

        public abstract bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, ref string message);
        
        protected void AddNoteAndSaveChanges(IOrderGroup orderGroup, string transactionType, string noteMessage)
        {
            var noteTitle = $"{PaymentMethod.PaymentMethod.FirstOrDefault()?.Name} {transactionType.ToLower()}";

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
