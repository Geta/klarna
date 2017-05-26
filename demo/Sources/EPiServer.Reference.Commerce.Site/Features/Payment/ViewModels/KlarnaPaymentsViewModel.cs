using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.ServiceLocation;
using Klarna.Payments.Extensions;
using Klarna.Payments;
using Klarna.Payments.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Managers;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class KlarnaPaymentsViewModel : PaymentMethodViewModel<KlarnaPaymentsPaymentMethod>, IKlarnaClientSession
    {
        private string _klarnaLogoUrl;

        public Injected<ICartService> InjectedCartService { get; set; }
        public ICartService CartService => InjectedCartService.Service;

        public Injected<ICurrentMarket> InjectedCurrentMarket { get; set; }
        public ICurrentMarket CurrentMarket => InjectedCurrentMarket.Service;

        public Injected<IKlarnaPaymentsService> InjectedKlarnaPaymentsService { get; set; }
        public IKlarnaPaymentsService KlarnaPaymentsService => InjectedKlarnaPaymentsService.Service;

        public KlarnaPaymentsViewModel()
        {

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
                    var config = paymentMethodDto.GetKlarnaPaymentsConfiguration(CurrentMarket.GetCurrentMarket().MarketId);
                    _klarnaLogoUrl = config.LogoUrl;
                }

                return _klarnaLogoUrl;
            }
        }

        public void InitializeValues()
        {
            var cart = CartService.LoadCart(CartService.DefaultCartName);
            ClientToken = KlarnaPaymentsService.GetClientToken(cart);
        }
    }
}