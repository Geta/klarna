using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Klarna.OrderManagement.Steps;

namespace Klarna.Checkout.Steps
{
    public class AuthorizePaymentStep : AuthorizePaymentStepBase
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(AuthorizePaymentStep));

        public AuthorizePaymentStep(IPayment payment) : base(payment)
        {
        }
        public override bool ProcessAuthorization(IPayment payment, IOrderGroup orderGroup, ref string message)
        {
            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Authorize completed");

            var orderId = orderGroup.Properties[Common.Constants.KlarnaOrderIdField]?.ToString();

            var order = KlarnaOrderService.GetOrder(orderId);
            if (order != null)
            {
                // TODO: check if fraude propery is pending 
            }

            return true;
        }
    }
}
