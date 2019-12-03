using System;
using System.Linq;
using System.Net;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Klarna.Rest.Core.Communication;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.MetaDataPlus;

namespace Klarna.OrderManagement.Steps
{
    public class CreditPaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CreditPaymentStep));

        public CreditPaymentStep(IPayment payment, MarketId marketId, KlarnaOrderServiceFactory klarnaOrderServiceFactory)
            : base(payment, marketId, klarnaOrderServiceFactory)
        {
        }

        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment, ref string message)
        {
            if (payment.TransactionType == TransactionType.Credit.ToString())
            {
                try
                {
                    var orderId = orderGroup.Properties[Common.Constants.KlarnaOrderIdField]?.ToString();
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
                catch (Exception ex) when (ex is ApiException || ex is WebException || ex is AggregateException)
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
                return Successor.Process(payment, orderForm, orderGroup, shipment, ref message);
            }
            return false;
        }
    }
}
