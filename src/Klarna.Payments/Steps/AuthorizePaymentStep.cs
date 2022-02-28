using System;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Klarna.Common.Configuration;
using Klarna.Common.Models;
using Klarna.OrderManagement;
using Klarna.OrderManagement.Steps;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Klarna.Payments.Steps
{
    public class AuthorizePaymentStep : AuthorizePaymentStepBase
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(AuthorizePaymentStep));

        private readonly IKlarnaPaymentsService _klarnaPaymentService;

        public AuthorizePaymentStep(
            IPayment payment, MarketId marketMarketId, IKlarnaOrderServiceFactory klarnaOrderServiceFactory, IKlarnaPaymentsService klarnaPaymentService, IConfigurationLoader configurationLoader, IPurchaseOrderProcessor purchaseOrderProcessor)
            : base(payment, marketMarketId, klarnaOrderServiceFactory, configurationLoader, purchaseOrderProcessor)
        {
            _klarnaPaymentService = klarnaPaymentService;
        }

        public override async Task<PaymentStepResult> ProcessAuthorization(IPayment payment, IOrderGroup orderGroup)
        {
            var paymentStepResult = new PaymentStepResult();

            var authorizationToken = payment.Properties[Constants.AuthorizationTokenPaymentField]?.ToString();
            if (!string.IsNullOrEmpty(authorizationToken))
            {
                try
                {
                    var result = await _klarnaPaymentService.CreateOrder(authorizationToken, orderGroup as ICart)
                        .ConfigureAwait(false);

                    orderGroup.Properties[Common.Constants.KlarnaOrderIdField] = result.OrderId;
                    payment.Properties[Common.Constants.FraudStatusPaymentField] = result.FraudStatus;
                    payment.Properties[Constants.KlarnaConfirmationUrlPaymentField] = result.RedirectUrl;

                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType,
                        $"Order created at Klarna, order id: {result.OrderId}, fraud status: {result.FraudStatus}");

                    if (result.FraudStatus == FraudStatus.REJECTED)
                    {
                        paymentStepResult.Message = "Klarna fraud status rejected";
                        payment.Status = PaymentStatus.Failed.ToString();
                    }
                    else
                    {
                        paymentStepResult.Status = true;
                    }
                }
                catch (Exception ex) when (ex is ApiException || ex is WebException || ex is AggregateException)
                {
                    var exceptionMessage = GetExceptionMessage(ex);

                    payment.Status = PaymentStatus.Failed.ToString();
                    paymentStepResult.Message = exceptionMessage;
                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {exceptionMessage}");
                    Logger.Error(exceptionMessage, ex);
                    paymentStepResult.Status = false;
                }
            }
            else
            {
                paymentStepResult.Message = "authorizationToken is null or empty";
            }
            return paymentStepResult;
        }
    }
}
