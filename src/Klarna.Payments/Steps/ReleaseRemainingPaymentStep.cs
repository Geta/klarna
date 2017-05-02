using System;
using System.Net;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Klarna.OrderManagement;
using Mediachase.Commerce.Orders;
using Refit;

namespace Klarna.Payments.Steps
{
    public class ReleaseRemainingPaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(ReleaseRemainingPaymentStep));

        public ReleaseRemainingPaymentStep(IPayment payment) : base(payment)
        {
        }

        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, ref string message)
        {
            if (payment.TransactionType == KlarnaAdditionalTransactionType.ReleaseRemainingAuthorization.ToString())
            {
                try
                {
                    var orderId = orderGroup.Properties[Constants.KlarnaOrderIdField]?.ToString();
                    if (!string.IsNullOrEmpty(orderId))
                    {
                        KlarnaOrderService.ReleaseRemaininAuthorization(orderId);

                        payment.Status = PaymentStatus.Processed.ToString();

                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Released remaining authorization at Klarna");

                        return true;
                    }
                }
                catch (Exception ex) when (ex is ApiException || ex is WebException)
                {
                    var exceptionMessage = GetExceptionMessage(ex);

                    payment.Status = PaymentStatus.Failed.ToString();
                    message = exceptionMessage;
                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {exceptionMessage}");
                    Logger.Error(exceptionMessage, ex);
                    return false;
                }
            }
            else if (Successor != null)
            {
                return Successor.Process(payment, orderForm, orderGroup, ref message);
            }
            return false;
        }
    }
}
