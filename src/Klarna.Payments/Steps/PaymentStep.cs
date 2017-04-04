using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Klarna.Payments.Extensions;

namespace Klarna.Payments.Steps
{
    public abstract class PaymentStep
    {
        protected Injected<IKlarnaService> KlarnaService;
        protected PaymentStep Successor;

        protected PaymentStep(IPayment payment)
        {

        }

        public void SetSuccessor(PaymentStep successor)
        {
            this.Successor = successor;
        }

        public abstract bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, ref string message);

        protected void AddNoteAndSaveChanges(IOrderGroup orderGroup, string noteTitle, string noteMessage)
        {
            orderGroup.AddNote(noteTitle, noteMessage);
        }
    }
}
