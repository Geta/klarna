using System;
using System.Linq;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;
using EPiServer.Logging;
using Klarna.OrderManagement.Steps;
using Klarna.Payments.Steps;

namespace Klarna.Payments
{
    public class KlarnaPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(KlarnaPaymentGateway));
        private IOrderForm _orderForm;

        public IOrderGroup OrderGroup { get; set; }

        public override bool ProcessPayment(Payment payment, ref string message)
        {
            OrderGroup = payment.Parent.Parent;
            _orderForm = payment.Parent;
            return ProcessPayment(payment as IPayment, ref message);
        }

        public bool ProcessPayment(IPayment payment, ref string message)
        {
            try
            {
                Logger.Debug("Klarna checkout gateway. Processing Payment ....");

                if (_orderForm == null)
                {
                    _orderForm = OrderGroup.Forms.FirstOrDefault(form => form.Payments.Contains(payment));
                }

                var authorizePaymentStep = new AuthorizePaymentStep(payment, OrderGroup.Market.MarketId);
                var cancelPaymentStep = new CancelPaymentStep(payment, OrderGroup.Market.MarketId);
                var capturePaymentStep = new CapturePaymentStep(payment, OrderGroup.Market.MarketId);
				var creditPaymentStep = new CreditPaymentStep(payment, OrderGroup.Market.MarketId);
                var releaseRemainingPaymentStep = new ReleaseRemainingPaymentStep(payment, OrderGroup.Market.MarketId);
                
                authorizePaymentStep.SetSuccessor(cancelPaymentStep);
                cancelPaymentStep.SetSuccessor(capturePaymentStep);
				capturePaymentStep.SetSuccessor(creditPaymentStep);
                creditPaymentStep.SetSuccessor(releaseRemainingPaymentStep);

                return authorizePaymentStep.Process(payment, _orderForm, OrderGroup, ref message);
            }
            catch (Exception ex)
            {
                Logger.Error("Process payment failed with error: " + ex.Message, ex);
                message = ex.Message;
                throw;
            }
        }
    }
}
