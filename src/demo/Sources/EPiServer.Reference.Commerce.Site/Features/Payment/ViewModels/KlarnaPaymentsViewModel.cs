using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.ServiceLocation;
using Klarna.Payments;
using Klarna.Payments.Models;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class KlarnaPaymentsViewModel : PaymentMethodViewModel<KlarnaPaymentsPaymentMethod>
    {
        public KlarnaPaymentsViewModel()
        {
            InitializeValues();
        }

        public string ClientToken { get; set; }
        public WidgetColorOptions ColorOptions { get; set; }

        public void InitializeValues()
        {
            var cartService = ServiceLocator.Current.GetInstance<ICartService>();
            var klarnaService = ServiceLocator.Current.GetInstance<IKlarnaService>();

            var cart = cartService.LoadCart(cartService.DefaultCartName);

            ClientToken = klarnaService.GetClientToken(cart);
            ColorOptions = klarnaService.GetWidgetColorOptions();
        }
    }
}