using System;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Klarna.Common.Configuration;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement.Steps
{
    public class CapturePaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CapturePaymentStep));

        public CapturePaymentStep(IPayment payment, MarketId marketId, IKlarnaOrderServiceFactory klarnaOrderServiceFactory, IConfigurationLoader configurationLoader)
            : base(payment, marketId, klarnaOrderServiceFactory, configurationLoader)
        {
        }

        /// <summary>
        /// When an order needs to be captured the cartridge needs to make an API Call
        /// POST /ordermanagement/v1/orders/{order_id}/captures for the amount which needs to be captured.
        ///     If it's a partial capture the API call will be the same just for the
        /// amount that should be captured. Many captures can be made up to the whole amount authorized.
        /// The shipment information can be added in this call or amended afterwards using the method
        /// "Add shipping info to a capture".
        ///     The amount captured never can be greater than the authorized.
        /// When an order is Partially Captured the status of the order in Klarna is PART_CAPTURED
        /// </summary>
        public override async Task<PaymentStepResult> Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment)
        {
            var paymentStepResult = new PaymentStepResult();

            if (payment.TransactionType == TransactionType.Capture.ToString())
            {
                var amount = AmountHelper.GetAmount(payment.Amount);
                var orderId = orderGroup.Properties[Common.Constants.KlarnaOrderIdField]?.ToString();
                if (!string.IsNullOrEmpty(orderId))
                {
                    try
                    {
                        var captureData = await KlarnaOrderService.CaptureOrder(orderId, amount, "Capture the payment", orderGroup, orderForm, payment, shipment).ConfigureAwait(false);
                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Captured {payment.Amount}, id {captureData.CaptureId}");
                        paymentStepResult.Status = true;
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
            }
            else if (Successor != null)
            {
                return await Successor.Process(payment, orderForm, orderGroup, shipment).ConfigureAwait(false);
            }

            return paymentStepResult;
        }
    }
}
