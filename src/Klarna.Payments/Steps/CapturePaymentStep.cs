using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Klarna.OrderManagement;
using Klarna.Payments.Models;
using Klarna.Rest.Models;
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
                var amount = PaymentStepHelper.GetAmount(payment.Amount);
                var orderId = orderGroup.Properties[Constants.KlarnaOrderIdField]?.ToString();
                if (!string.IsNullOrEmpty(orderId))
                {
                    try
                    {
                        // TODO shipping info
                        // TODO order lines
                        var captureData = KlarnaOrderService.CaptureOrder(orderId, amount, "Capture the payment");
                        AddNoteAndSaveChanges(orderGroup, $"Payment - Captured: {captureData.CaptureId}");
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
                        AddNoteAndSaveChanges(orderGroup,
                            "Payment Captured - Error",
                            exceptionMessage);
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

    /// <summary>
    /// Payment step helper class
    /// </summary>
    public static class PaymentStepHelper
    {
        /// <summary>
        /// Get amount as string value (without decimals)
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static int GetAmount(decimal amount)
        {
            return (int)Math.Round(amount * 100);
        }
    }
}
