using System;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Klarna.Common.Configuration;
using Klarna.Common.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement.Steps
{
    public class CancelPaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CancelPaymentStep));

        public CancelPaymentStep(IPayment payment, MarketId marketId, IKlarnaOrderServiceFactory klarnaOrderServiceFactory, IConfigurationLoader configurationLoader)
            : base(payment, marketId, klarnaOrderServiceFactory, configurationLoader)
        {
        }

        public override async Task<PaymentStepResult> Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment)
        {
            var paymentStepResult = new PaymentStepResult();

            if (payment.TransactionType == TransactionType.Void.ToString())
            {
                try
                {
                    var orderId = orderGroup.Properties[Common.Constants.KlarnaOrderIdField]?.ToString();
                    if (!string.IsNullOrEmpty(orderId))
                    {
                        await KlarnaOrderService.CancelOrder(orderId).ConfigureAwait(false);

                        payment.Status = PaymentStatus.Processed.ToString();

                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Order cancelled at Klarna");

                        paymentStepResult.Status = true;
                    }
                }
                catch (Exception ex) when (ex is ApiException || ex is WebException)
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
