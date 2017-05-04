using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.ServiceLocation;
using Klarna.Payments;
using Klarna.Payments.Extensions;
using Klarna.Payments.Models;
using Mediachase.Commerce.Orders.Managers;
using Newtonsoft.Json;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class KlarnaPaymentsViewModel : PaymentMethodViewModel<KlarnaPaymentsPaymentMethod>, IKlarnaClientSession
    {
        private string _klarnaLogoUrl;

        public Injected<ICartService> InjectedCartService { get; set; }
        public ICartService CartService => InjectedCartService.Service;

        public Injected<IKlarnaService> InjectedKlarnaService { get; set; }
        public IKlarnaService KlarnaService => InjectedKlarnaService.Service;

        public KlarnaPaymentsViewModel()
        {
            InitializeValues();
        }

        public string ClientToken { get; set; }

        public string KlarnaLogoUrl
        {
            get
            {
                if (_klarnaLogoUrl != null)
                {
                    return _klarnaLogoUrl;
                }

                if (PaymentMethod != null)
                {
                    var paymentMethodDto = PaymentManager.GetPaymentMethod(PaymentMethod.PaymentMethodId);
                    _klarnaLogoUrl = paymentMethodDto.GetParameter(Constants.KlarnaLogoUrlField, string.Empty);
                }

                return _klarnaLogoUrl;
            }
        }

        public void InitializeValues()
        {
            var cart = CartService.LoadCart(CartService.DefaultCartName);
            ClientToken = KlarnaService.GetClientToken(cart);
        }
    }
}