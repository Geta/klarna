using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Klarna.Common.Configuration;
using Klarna.Common.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.MetaDataPlus;

namespace Klarna.OrderManagement.Steps
{
    public class CreditPaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CreditPaymentStep));

        public CreditPaymentStep(IPayment payment, MarketId marketId, IKlarnaOrderServiceFactory klarnaOrderServiceFactory, IConfigurationLoader configurationLoader)
            : base(payment, marketId, klarnaOrderServiceFactory, configurationLoader)
        {
        }

        public override async Task<PaymentStepResult> Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment)
        {
            var paymentStepResult = new PaymentStepResult();

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
                            var returnForm = purchaseOrder.ReturnForms.FirstOrDefault(x=> ((OrderForm)x).Status == ReturnFormStatus.AwaitingCompletion.ToString());

                            if (returnForm != null)
                            {
                                await KlarnaOrderService.Refund(orderId, orderGroup, (OrderForm)returnForm, payment).ConfigureAwait(false);
                            }
                        }
                        payment.Status = PaymentStatus.Processed.ToString();

                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Refunded {payment.Amount}");

                        paymentStepResult.Status = true;
                    }
                }
                catch (Exception ex) when (ex is ApiException || ex is WebException || ex is AggregateException)
                {
                    var exceptionMessage = GetExceptionMessage(ex);

                    payment.Status = PaymentStatus.Failed.ToString();
                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {exceptionMessage}");
                    Logger.Error(exceptionMessage, ex);

                    paymentStepResult.Message = exceptionMessage;
                }
            }
            else if (Successor != null)
            {
                return await Successor.Process(payment, orderForm, orderGroup, shipment).ConfigureAwait(false);
            }

            return paymentStepResult;
        }
    }
}
