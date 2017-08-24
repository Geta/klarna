using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;

namespace EPiServer.Reference.Commerce.Shared
{
    public class GenericCreditCardPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {
            var creditCardPayment = (CreditCardPayment)payment;

            if (!creditCardPayment.CreditCardNumber.EndsWith("4"))
            {
                return PaymentProcessingResult.CreateSuccessfulResult("Invalid credit card number.");
            }

            return PaymentProcessingResult.CreateSuccessfulResult("Payment successful.");
        }

        /// <inheritdoc/>
        public override bool ProcessPayment(Payment payment, ref string message)
        {
            var creditCardPayment = (CreditCardPayment)payment;

            if (!creditCardPayment.CreditCardNumber.EndsWith("4"))
            {
                message = "Invalid credit card number.";
                return false;
            }

            return true;
        }
    }
}