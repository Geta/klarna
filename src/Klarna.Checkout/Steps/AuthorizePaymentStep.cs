using System;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Klarna.OrderManagement;
using Klarna.OrderManagement.Steps;
using Klarna.Payments.Models;
using Klarna.Rest.Transport;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Klarna.Checkout.Steps
{
    public class AuthorizePaymentStep : AuthorizePaymentStepBase
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(AuthorizePaymentStep));

        public AuthorizePaymentStep(IPayment payment, MarketId marketId, KlarnaOrderServiceFactory klarnaOrderServiceFactory)
            : base(payment, marketId, klarnaOrderServiceFactory)
        {
        }

        public override bool ProcessAuthorization(IPayment payment, IOrderGroup orderGroup, ref string message)
        {
            var orderId = orderGroup.Properties[Constants.KlarnaCheckoutOrderIdCartField]?.ToString();

            try
            {
                var result = Task
                    .Run(() => KlarnaOrderService.GetOrder(orderId))
                    .Result;

                if (result != null)
                {
                    payment.Properties[Common.Constants.FraudStatusPaymentField] = result.FraudStatus;

                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Fraud status: {result.FraudStatus}");

                    if (result.FraudStatus == FraudStatus.REJECTED)
                    {
                        message = "Klarna fraud status rejected";
                        payment.Status = PaymentStatus.Failed.ToString();
                    }
                    else
                    {
                        return true;
                    }
                }

                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Authorize completed");
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

            return true;
        }
    }
}
