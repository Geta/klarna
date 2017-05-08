using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.ServiceLocation;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class KlarnaCheckoutViewModel : PaymentMethodViewModel<KlarnaCheckoutPaymentMethod>
    {
        public Injected<ICartService> InjectedCartService { get; set; }
        public ICartService CartService => InjectedCartService.Service;

        public KlarnaCheckoutViewModel()
        {
            InitializeValues();
        }

        public void InitializeValues()
        {
        }
    }
}