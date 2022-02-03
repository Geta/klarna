using System;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Klarna.Common.Models;
using Klarna.OrderManagement;
using Klarna.OrderManagement.Steps;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Klarna.Checkout.Steps
{
    public class AuthorizePaymentStep : AuthorizePaymentStepBase
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(AuthorizePaymentStep));

        public AuthorizePaymentStep(IPayment payment, MarketId marketId, IKlarnaOrderServiceFactory klarnaOrderServiceFactory)
            : base(payment, marketId, klarnaOrderServiceFactory)
        {
        }

        public override async Task<PaymentStepResult> ProcessAuthorization(IPayment payment, IOrderGroup orderGroup)
        {
            var paymentStepResult = new PaymentStepResult();
            var orderId = orderGroup.Properties[Constants.KlarnaCheckoutOrderIdCartField]?.ToString();

            try
            {
                var result = await KlarnaOrderService.GetOrder(orderId).ConfigureAwait(false);

                if (result != null)
                {
                    payment.Properties[Common.Constants.FraudStatusPaymentField] = result.FraudStatus;

                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Fraud status: {result.FraudStatus}");

                    if (result.FraudStatus == OrderManagementFraudStatus.REJECTED)
                    {
                        paymentStepResult.Message = "Klarna fraud status rejected";
                        payment.Status = PaymentStatus.Failed.ToString();
                    }
                    else
                    {
                        paymentStepResult.Status = true;
                    }
                }

                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Authorize completed");
            }
            catch (Exception ex) when (ex is ApiException || ex is WebException || ex is AggregateException)
            {
                var exceptionMessage = GetExceptionMessage(ex);

                payment.Status = PaymentStatus.Failed.ToString();
                paymentStepResult.Message = exceptionMessage;
                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {exceptionMessage}");
                Logger.Error(exceptionMessage, ex);
            }

            return paymentStepResult;
        }
    }
}
