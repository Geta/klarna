using System;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Klarna.Payments.Models;
using Mediachase.Commerce.Orders;
using Refit;

namespace Klarna.Payments.Steps
{
    public class AuthorizePaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(AuthorizePaymentStep));

        public AuthorizePaymentStep(IPayment payment) : base(payment)
        {
        }
        
        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, ref string message)
        {
            if (payment.TransactionType == TransactionType.Authorization.ToString())
            {
                // Fraud status update
                if (payment.Status == PaymentStatus.Pending.ToString() && !string.IsNullOrEmpty(orderGroup.Properties[Common.Constants.KlarnaOrderIdField]?.ToString()))
                {
                    return ProcessFraudUpdate(payment, orderGroup, ref message);
                }
                else
                {
                    return ProcessAuthorization(payment, orderGroup, ref message);
                }
            }
            else if (Successor != null)
            {
                return Successor.Process(payment, orderForm, orderGroup, ref message);
            }
            return false;
        }

        private bool ProcessFraudUpdate(IPayment payment, IOrderGroup orderGroup, ref string message)
        {
            if (payment.Properties[Common.Constants.FraudStatusPaymentMethodField]?.ToString() == NotificationFraudStatus.FRAUD_RISK_ACCEPTED.ToString())
            {
                payment.Status = PaymentStatus.Processed.ToString();

                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Klarna fraud risk accepted");

                return true;
            }
            else if (payment.Properties[Common.Constants.FraudStatusPaymentMethodField]?.ToString() == NotificationFraudStatus.FRAUD_RISK_REJECTED.ToString())
            {
                payment.Status = PaymentStatus.Failed.ToString();

                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Klarna fraud risk rejected");

                return false;
            }
            else if (payment.Properties[Common.Constants.FraudStatusPaymentMethodField]?.ToString() == NotificationFraudStatus.FRAUD_RISK_STOPPED.ToString())
            {
                //TODO Fraud status stopped

                payment.Status = PaymentStatus.Failed.ToString();

                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Klarna fraud risk stopped");

                return false;
            }
            message = $"Can't process authorization. Unknown fraud notitication: {payment.Properties[Common.Constants.FraudStatusPaymentMethodField]} or no fraud notifications received so far.";
            return false;
        }

        private bool ProcessAuthorization(IPayment payment, IOrderGroup orderGroup, ref string message)
        {
            var authorizationToken = payment.Properties[Constants.AuthorizationTokenPaymentMethodField]?.ToString();
            if (!string.IsNullOrEmpty(authorizationToken))
            {
                try
                {
                    var result = Task
                        .Run(() => KlarnaService.Service.CreateOrder(authorizationToken, orderGroup as ICart))
                        .Result;

                    orderGroup.Properties[Common.Constants.KlarnaOrderIdField] = result.OrderId;
                    payment.Properties[Common.Constants.FraudStatusPaymentMethodField] = result.FraudStatus;
                    payment.Properties[Constants.KlarnaConfirmationUrlField] = result.RedirectUrl;

                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType,
                        $"Order created at Klarna, order id: {result.OrderId}, fraud status: {result.FraudStatus}");

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
            else
            {
                message = "authorizationToken is null or empty";
            }
            return false;
        }
    }
}
