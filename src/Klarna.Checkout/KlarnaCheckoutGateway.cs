using System;
using System.Linq;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Klarna.Checkout.Steps;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Klarna.OrderManagement;
using Klarna.OrderManagement.Steps;

namespace Klarna.Checkout
{
    public class KlarnaCheckoutGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(KlarnaCheckoutGateway));
        private IOrderForm _orderForm;

        public IOrderGroup OrderGroup { get; set; }

        internal Injected<IKlarnaOrderServiceFactory> InjectedKlarnaOrderServiceFactory { get; set; }
        private IKlarnaOrderServiceFactory KlarnaOrderServiceFactory => InjectedKlarnaOrderServiceFactory.Service;

        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {
            OrderGroup = orderGroup;
            _orderForm = orderGroup.GetFirstForm();
            var result = ProcessPayment(payment);
            var message = result.Message;
            return result.Status
                ? PaymentProcessingResult.CreateSuccessfulResult(message)
                : PaymentProcessingResult.CreateUnsuccessfulResult(message);
        }

        public override bool ProcessPayment(Payment payment, ref string message)
        {
            OrderGroup = payment.Parent.Parent;
            _orderForm = payment.Parent;
            var result = ProcessPayment(payment);
            message = result.Message;

            return result.Status;
        }

        public PaymentStepResult ProcessPayment(IPayment payment)
        {
            try
            {
                Logger.Debug("Klarna checkout gateway. Processing Payment ....");

                if (_orderForm == null)
                {
                    _orderForm = OrderGroup.Forms.FirstOrDefault(form => form.Payments.Contains(payment));
                }

                var authorizePaymentStep = new AuthorizePaymentStep(payment, OrderGroup.MarketId, KlarnaOrderServiceFactory);
                var capturePaymentStep = new CapturePaymentStep(payment, OrderGroup.MarketId, KlarnaOrderServiceFactory);
                var creditPaymentStep = new CreditPaymentStep(payment, OrderGroup.MarketId, KlarnaOrderServiceFactory);
                var cancelPaymentStep = new CancelPaymentStep(payment, OrderGroup.MarketId, KlarnaOrderServiceFactory);

                authorizePaymentStep.SetSuccessor(capturePaymentStep);
                capturePaymentStep.SetSuccessor(creditPaymentStep);
                creditPaymentStep.SetSuccessor(cancelPaymentStep);

                return AsyncHelper.RunSync(() => authorizePaymentStep.Process(payment, _orderForm, OrderGroup, OrderGroup.GetFirstShipment()));
            }
            catch (Exception ex)
            {
                Logger.Error("Process checkout failed with error: " + ex.Message, ex);

                var paymentStepResult = new PaymentStepResult {Message = ex.Message};
                return paymentStepResult;
            }
        }
    }
}
