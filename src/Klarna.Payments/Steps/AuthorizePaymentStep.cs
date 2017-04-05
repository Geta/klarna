using System;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.Logging;
using Klarna.Payments.Extensions;
using Klarna.Payments.Models;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;

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
                        // TODO: var result = KlarnaService.Service.CreateOrder(authorizationToken, orderGroup).Result;
                        var result = new CreateOrderResponse
                        {
                            FraudStatus = FraudStatus.ACCEPTED,
                            OrderId = "1234567890"
                        };

                        var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaPaymentSystemKeyword, ContentLanguage.PreferredCulture.Name);
                        if (paymentMethod != null)
                        {
                            result.RedirectUrl = paymentMethod.GetParameter(Constants.ConfirmationUrlField);
                        }

                        orderGroup.Properties[Constants.KlarnaOrderIdField] = result.OrderId;
                        payment.Properties[Constants.FraudStatusPaymentMethodField] = result.FraudStatus;
                        payment.Properties[Constants.KlarnaConfirmationUrlField] = result.RedirectUrl;

                        AddNoteAndSaveChanges(orderGroup, "Payment Authorize",
                            $"Place order at Klarna, orderid: {result.OrderId}, fraud status: {result.FraudStatus}");

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

                        AddNoteAndSaveChanges(orderGroup, "Payment Authorize - Error", ex.Message);
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
