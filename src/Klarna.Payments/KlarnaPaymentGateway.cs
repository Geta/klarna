using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;

namespace Klarna.Payments
{
    public class KlarnaPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        public override bool ProcessPayment(Payment payment, ref string message)
        {
            throw new System.NotImplementedException();
        }

        public bool ProcessPayment(IPayment payment, ref string message)
        {
            return ProcessPayment((Payment)payment, ref message);
        }
    }
}
