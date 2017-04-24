using System;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Klarna.Payments.Models;
using Mediachase.Commerce.Orders;

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
            if (payment.TransactionType == "Authorization")
            {
                // Fraud status update
                if (payment.Status == PaymentStatus.Pending.ToString() && !string.IsNullOrEmpty(orderGroup.Properties[Constants.KlarnaOrderIdField]?.ToString()))
                {
                    if (payment.Properties[Constants.FraudStatusPaymentMethodField]?.ToString() == NotificationFraudStatus.FRAUD_RISK_ACCEPTED.ToString())
                    {
                        payment.Status = PaymentStatus.Processed.ToString();

                        AddNoteAndSaveChanges(orderGroup, "Payment authorization", "Fraud risk accepted");

                        return true;
                    }
                    else if (payment.Properties[Constants.FraudStatusPaymentMethodField]?.ToString() == NotificationFraudStatus.FRAUD_RISK_REJECTED.ToString())
                    {
                        payment.Status = PaymentStatus.Failed.ToString();

                        AddNoteAndSaveChanges(orderGroup, "Payment authorization", "Fraud risk rejected");

                        return false;
                    }
                    //TODO Fraud status stopped
                }
                else
                {
                    var authorizationToken = payment.Properties[Constants.AuthorizationTokenPaymentMethodField]?.ToString();
                    if (!string.IsNullOrEmpty(authorizationToken))
                    {
                        try
                        {
                            var result = Task.Run(() => KlarnaService.Service.CreateOrder(authorizationToken, orderGroup as ICart)).Result;

                            orderGroup.Properties[Constants.KlarnaOrderIdField] = result.OrderId;
                            payment.Properties[Constants.FraudStatusPaymentMethodField] = result.FraudStatus;
                            payment.Properties[Constants.KlarnaConfirmationUrlField] = result.RedirectUrl;

                            AddNoteAndSaveChanges(orderGroup, "Payment authorization", $"Order created at Klarna, order id: {result.OrderId}, fraud status: {result.FraudStatus}");

                            if (result.FraudStatus == FraudStatus.REJECTED)
                            {
                                message = "Klarna fraud status reject";
                                payment.Status = PaymentStatus.Failed.ToString();
                            }
                            else
                            {
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            payment.Status = PaymentStatus.Failed.ToString();
                            Logger.Error(ex.Message, ex);

                            AddNoteAndSaveChanges(orderGroup, "Payment authorization - Error", ex.Message);
                        }
                    }
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
