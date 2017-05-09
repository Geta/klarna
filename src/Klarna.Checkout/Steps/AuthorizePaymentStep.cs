using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Mediachase.Commerce.Orders;

namespace Klarna.Checkout.Steps
{
    public class AuthorizePaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(AuthorizePaymentStep));

        public AuthorizePaymentStep(IPayment payment) : base(payment)
        {
        }
        
        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, ref string message)
        {
            /*if (payment.TransactionType == TransactionType.Authorization.ToString())
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
            return false;*/
            return true;
        }

        private bool ProcessFraudUpdate(IPayment payment, IOrderGroup orderGroup, ref string message)
        {
            /*if (payment.Properties[Constants.FraudStatusPaymentMethodField]?.ToString() == NotificationFraudStatus.FRAUD_RISK_ACCEPTED.ToString())
            {
                payment.Status = PaymentStatus.Processed.ToString();

                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Klarna fraud risk accepted");

                return true;
            }
            else if (payment.Properties[Constants.FraudStatusPaymentMethodField]?.ToString() == NotificationFraudStatus.FRAUD_RISK_REJECTED.ToString())
            {
                payment.Status = PaymentStatus.Failed.ToString();

                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Klarna fraud risk rejected");

                return false;
            }
            else if (payment.Properties[Constants.FraudStatusPaymentMethodField]?.ToString() == NotificationFraudStatus.FRAUD_RISK_STOPPED.ToString())
            {
                //TODO Fraud status stopped

                payment.Status = PaymentStatus.Failed.ToString();

                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Klarna fraud risk stopped");

                return false;
            }
            message = $"Can't process authorization. Unknown fraud notitication: {payment.Properties[Constants.FraudStatusPaymentMethodField]} or no fraud notifications received so far.";*/
            return false;
        }

        private bool ProcessAuthorization(IPayment payment, IOrderGroup orderGroup, ref string message)
        {
            return true;
        }
    }
}
