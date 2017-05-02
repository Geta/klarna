using System;
using System.Net;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Klarna.Payments.Helpers;
using Klarna.Rest.Transport;
using Mediachase.Commerce.Orders;

namespace Klarna.Payments.Steps
{
    public class CapturePaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CapturePaymentStep));

        public CapturePaymentStep(IPayment payment) : base(payment)
        {
        }

        /// <summary>
        /// When an order needs to be captured the cartridge needs to make an API Call 
        /// POST /ordermanagement/v1/orders/{order_id}/captures for the amount which needs to be captured.
        ///     If it's a partial capture the API call will be the same just for the 
        /// amount that should be captured. Many captures can be made up to the whole amount authorized.
        /// The shipment information can be added in this call or amended aftewards using the method 
        /// "Add shipping info to a capture".
        ///     The amount captured never can be greater than the authorized.
        /// When an order is Partally Captured the status of the order in Klarna is PART_CAPTURED
        /// </summary>
        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, ref string message)
        {
            if (payment.TransactionType == TransactionType.Capture.ToString())
            {
                var amount = AmountHelper.GetAmount(payment.Amount);
                var orderId = orderGroup.Properties[Constants.KlarnaOrderIdField]?.ToString();
                if (!string.IsNullOrEmpty(orderId))
                {
                    try
                    {
                        var captureData = KlarnaOrderService.CaptureOrder(orderId, amount, "Capture the payment", orderGroup, orderForm, payment);
                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Captured {payment.Amount}, id {captureData.CaptureId}");
                    }
                    catch (Exception ex) when (ex is ApiException || ex is WebException)
                    {
                        var exceptionMessage = string.Empty;
                        switch (ex)
                        {
                            case ApiException apiException:
                                exceptionMessage = 
                                    $"Payment - Captured - Error: " +
                                    $"{apiException.ErrorMessage.CorrelationId} " +
                                    $"{apiException.ErrorMessage.ErrorCode} " +
                                    $"{apiException.ErrorMessage.ErrorMessages}";
                                break;
                            case WebException webException:
                                exceptionMessage =
                                    $"Payment - Captured - Error: {webException.Message}";
                                break;
                        }

                        payment.Status = PaymentStatus.Failed.ToString();
                        message = exceptionMessage;
                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {exceptionMessage}");
                        return false;
                    }
                }

                return true;
            }
            else if (Successor != null)
            {
                return Successor.Process(payment, orderForm, orderGroup, ref message);
            }
            return false;
        }
    }
}
