using System;
using System.Linq;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;
using EPiServer.Logging;
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
            try
            {
                Logger.Debug("Netaxept checkout gateway. Processing Payment ....");

                OrderGroup = payment.Parent.Parent;
                _orderForm = payment.Parent;
                return ProcessPayment(payment as IPayment, ref message);
            }
            catch (Exception ex)
            {
                Logger.Error("Process payment failed with error: " + ex.Message, ex);
                message = ex.Message;
                throw;
            }
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

                var authorizePaymentStep = new AuthorizePaymentStep(payment);

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
