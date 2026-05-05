using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Klarna.Common.Extensions;
using Klarna.Common.Models;
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

        protected PaymentStep(IPayment payment, MarketId marketId, KlarnaOrderServiceFactory klarnaOrderServiceFactory)
        {
            MarketId = marketId;

            PaymentMethod = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
            if (PaymentMethod != null)
            {
                KlarnaOrderService = klarnaOrderServiceFactory.Create(PaymentMethod.GetConnectionConfiguration(marketId));
            }
        }

        public void SetSuccessor(PaymentStep successor)
        {
            this.Successor = successor;
        }

        public abstract Task<PaymentStepResult> Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment);

        protected void AddNoteAndSaveChanges(IOrderGroup orderGroup, string transactionType, string noteMessage)
        {
            var noteTitle = $"{PaymentMethod.PaymentMethod.FirstOrDefault()?.Name} {transactionType.ToLower()}";

            orderGroup.AddNote(noteTitle, $"Payment {transactionType.ToLower()}: {noteMessage}");
        }

        protected string GetExceptionMessage(Exception ex)
        {
            try
            {
                switch (ex)
                {
                    case AggregateException aggregateException:
                        return string.Join("; ",
                            aggregateException.InnerExceptions.Select(GetExceptionMessage));
                    case ApiException apiException:
                        var errorMessage = apiException.ErrorMessage;
                        if (errorMessage == null)
                        {
                            return apiException.Message ?? string.Empty;
                        }
                        var messages = errorMessage.ErrorMessages != null
                            ? string.Join(", ", errorMessage.ErrorMessages)
                            : string.Empty;
                        return $"{errorMessage.CorrelationId} {errorMessage.ErrorCode} {messages}";
                    case WebException webException:
                        return webException.Message;
                }
                return string.Empty;
            }
            catch
            {
                return ex?.ToString() ?? string.Empty;
            }
        }
    }
}
