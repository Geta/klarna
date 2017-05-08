using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.ServiceLocation;
using Klarna.Checkout;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class KlarnaCheckoutViewModel : PaymentMethodViewModel<KlarnaCheckoutPaymentMethod>
    {
        public Injected<ICartService> InjectedCartService { get; set; }
        public ICartService CartService => InjectedCartService.Service;

        public Injected<IKlarnaCheckoutService> InjectedKlarnaCheckoutService { get; set; }
        public IKlarnaCheckoutService KlarnaPaymentsService => InjectedKlarnaCheckoutService.Service;

        public KlarnaCheckoutViewModel()
        {
            InitializeValues();
        }

        public string HtmlSnippet { get; set; }

        public void InitializeValues()
        {
            var cart = CartService.LoadCart(CartService.DefaultCartName);

            HtmlSnippet = KlarnaPaymentsService.GetOrder(cart)?.HtmlSnippet;
        }
    }
}