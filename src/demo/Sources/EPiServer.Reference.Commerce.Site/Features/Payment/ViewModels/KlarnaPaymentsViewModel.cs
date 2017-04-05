using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.ServiceLocation;
using Klarna.Payments;
using Klarna.Payments.Models;
using Newtonsoft.Json;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class KlarnaPaymentsViewModel : PaymentMethodViewModel<KlarnaPaymentsPaymentMethod>, IKlarnaClientSession
    {
        public Injected<ICartService> InjectedCartService { get; set; }
        public ICartService CartService => InjectedCartService.Service;

        public Injected<IKlarnaService> InjectedKlarnaService { get; set; }
        public IKlarnaService KlarnaService => InjectedKlarnaService.Service;

        public KlarnaPaymentsViewModel()
        {
            InitializeValues();
        }

        public string ClientToken { get; set; }
        public string SessionId { get; set; }

        public void InitializeValues()
        {
            var cart = CartService.LoadCart(CartService.DefaultCartName);
            ClientToken = KlarnaService.GetClientToken(cart);
            SessionId = KlarnaService.GetSessionId(cart);
        }
    }
}