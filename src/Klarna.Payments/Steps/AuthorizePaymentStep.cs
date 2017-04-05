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
                var authorizationToken = payment.Properties[Constants.AuthorizationTokenPaymentMethodField]?.ToString();
                if (!string.IsNullOrEmpty(authorizationToken))
                {
                    try
                    {

                        var result = Task.Run(() => KlarnaService.Service.CreateOrder(authorizationToken, orderGroup).Result).Result;

                        /*var result = new CreateOrderResponse
                        {
                            FraudStatus = FraudStatus.ACCEPTED,
                            OrderId = "1234567890"
                        };

                        var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaPaymentSystemKeyword, ContentLanguage.PreferredCulture.Name);
                        if (paymentMethod != null)
                        {
                            result.RedirectUrl = paymentMethod.GetParameter(Constants.ConfirmationUrlField);
                        }*/

                        orderGroup.Properties[Constants.KlarnaOrderIdField] = result.OrderId;
                        payment.Properties[Constants.FraudStatusPaymentMethodField] = result.FraudStatus;
                        payment.Properties[Constants.KlarnaConfirmationUrlField] = result.RedirectUrl;

                        AddNoteAndSaveChanges(orderGroup, "Payment authorization", $"Place order at Klarna, orderid: {result.OrderId}, fraud status: {result.FraudStatus}");

                        if (result.FraudStatus == FraudStatus.REJECTED)
                        {
                            message = "Klarna fraud status reject";
                            payment.Status = PaymentStatus.Failed.ToString();

                            return false;
                        }
                        else // either accept or pending
                        {
                            payment.Status = PaymentStatus.Pending.ToString();

                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message, ex);

                        AddNoteAndSaveChanges(orderGroup, "Payment authorization - Error", ex.Message);
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
