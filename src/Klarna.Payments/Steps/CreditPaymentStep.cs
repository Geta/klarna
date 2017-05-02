using System;
using System.Linq;
using System.Net;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Mediachase.Commerce.Orders;
using Mediachase.MetaDataPlus;
using Refit;

namespace Klarna.Payments.Steps
{
    public class CreditPaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CreditPaymentStep));

        public CreditPaymentStep(IPayment payment) : base(payment)
        {
        }

        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, ref string message)
        {
            if (payment.TransactionType == TransactionType.Credit.ToString())
            {
                try
                {
                    var orderId = orderGroup.Properties[Constants.KlarnaOrderIdField]?.ToString();
                    if (!string.IsNullOrEmpty(orderId))
                    {
                        var purchaseOrder = orderGroup as IPurchaseOrder;
                        if (purchaseOrder != null)
                        {
                            var returnForm = purchaseOrder.ReturnForms.FirstOrDefault(x=> ((OrderForm)x).Status == ReturnFormStatus.Complete.ToString() && ((OrderForm)x).ObjectState == MetaObjectState.Modified);

                            if (returnForm != null)
                            {
                                KlarnaOrderService.Refund(orderId, orderGroup, (OrderForm)returnForm, payment);
                            }
                        }
                        payment.Status = PaymentStatus.Processed.ToString();

                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Refunded {payment.Amount}");

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
