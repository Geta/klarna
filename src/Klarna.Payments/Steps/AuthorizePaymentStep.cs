using System;
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
            var authorizationToken = payment.Properties[Constants.AuthorizationTokenPaymentMethodField]?.ToString();
            if (!string.IsNullOrEmpty(authorizationToken))
            {
                try
                {
                    var result = KlarnaService.Service.CreateOrder(authorizationToken, orderGroup).Result;

                    orderGroup.Properties[Constants.KlarnaOrderIdField] = result.OrderId;

                    payment.Properties[Constants.FraudStatusPaymentMethodField] = result.FraudStatus;

                    AddNoteAndSaveChanges(orderGroup, "Payment Authorize", $"Place order at Klarna, orderid: {result.OrderId}, fraud status: {result.FraudStatus}");

                    if (result.FraudStatus == FraudStatus.REJECTED)
                    {
                        message = "Klarna fraud status reject";
                        payment.Status = PaymentStatus.Failed.ToString();

                        return false;
                    }
                    else // either acceptd or pending
                    {
                        payment.Status = PaymentStatus.Pending.ToString();

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message, ex);

                    AddNoteAndSaveChanges(orderGroup, "Payment Authorize - Error", ex.Message);
                }
            }
            return false;
        }
    }
}
