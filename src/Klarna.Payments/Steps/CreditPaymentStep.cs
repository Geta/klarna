using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Mediachase.Commerce.Orders;

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
                            var returnForm = purchaseOrder.ReturnForms.FirstOrDefault();
                            if (returnForm != null)
                            {
                                KlarnaOrderService.Refund(orderId, orderGroup, (OrderForm)orderForm, payment);
                            }
                        }
                        payment.Status = PaymentStatus.Processed.ToString();

                        AddNoteAndSaveChanges(orderGroup, "Payment credit", $"Klarna - refund: amount {payment.Amount}");

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    payment.Status = PaymentStatus.Failed.ToString();
                    Logger.Error(ex.Message, ex);

                    AddNoteAndSaveChanges(orderGroup, "Payment credit - Error", ex.Message);
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
