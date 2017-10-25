using EPiServer.Commerce.Order;
using Klarna.OrderManagement;
using Klarna.OrderManagement.Steps;
using Mediachase.Commerce;

namespace Klarna.Checkout.Steps
{
    public class AuthorizePaymentStep : AuthorizePaymentStepBase
    {
        public AuthorizePaymentStep(IPayment payment, MarketId marketId, KlarnaOrderServiceFactory klarnaOrderServiceFactory)
            : base(payment, marketId, klarnaOrderServiceFactory)
        {
        }
        public override bool ProcessAuthorization(IPayment payment, IOrderGroup orderGroup, ref string message)
        {
            AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Authorize completed");
            
            return true;
        }
    }
}
