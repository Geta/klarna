using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Klarna.OrderManagement;
using Klarna.Payments.Extensions;
using Mediachase.Commerce.Orders.Managers;

namespace Klarna.Payments.Steps
{
    public abstract class PaymentStep
    {
        protected Injected<IKlarnaService> KlarnaService;
        protected IKlarnaOrderService KlarnaOrderService;
        protected PaymentStep Successor;

        protected PaymentStep(IPayment payment)
        {
            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaPaymentSystemKeyword, ContentLanguage.PreferredCulture.Name);
            if (paymentMethod != null)
            {
                var username = paymentMethod.GetParameter(Constants.KlarnaUsernameField, string.Empty);
                var password = paymentMethod.GetParameter(Constants.KlarnaPasswordField, string.Empty);
                var apiUrl = paymentMethod.GetParameter(Constants.KlarnaApiUrlField, string.Empty);

                KlarnaOrderService = new KlarnaOrderService(username, password, apiUrl);
            }
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
