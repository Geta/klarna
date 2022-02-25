using EPiServer;
using EPiServer.Web.Mvc;
using Foundation.Features.Checkout.Payments;
using Foundation.Features.Checkout.Services;
using Foundation.Features.Checkout.ViewModels;
using Foundation.Features.Settings;
using Foundation.Infrastructure.Cms.Settings;
using Klarna.Checkout;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Features.Checkout
{
    public class KlarnaCheckoutPageController : PageController<KlarnaCheckoutPage>
    {
        private readonly ICartService _cartService;
        private readonly CheckoutViewModelFactory _checkoutViewModelFactory;
        private readonly ISettingsService _settingsService;
        private readonly IContentLoader _contentLoader;


        public KlarnaCheckoutPageController(ICartService cartService, CheckoutViewModelFactory checkoutViewModelFactory, ISettingsService settingsService, IContentLoader contentLoader)
        {
            _cartService = cartService;
            _checkoutViewModelFactory = checkoutViewModelFactory;
            _contentLoader = contentLoader;
            _settingsService = settingsService;
        }

        public IActionResult Index(KlarnaCheckoutPage currentPage)
        {
            if (currentPage == null)
            {
                currentPage = _contentLoader.Get<KlarnaCheckoutPage>(_settingsService.GetSiteSettings<ReferencePageSettings>().KlarnaCheckoutPage);
            }

            var cartWithValidation = _cartService.LoadCart(_cartService.DefaultCartName, true);

            var viewModel = _checkoutViewModelFactory.CreateCheckoutViewModel(cartWithValidation.Cart, currentPage, Constants.KlarnaCheckoutSystemKeyword.GetPaymentMethod());

            ((KlarnaCheckoutPaymentOption)viewModel.Payment).Initialize();

            return View("KlarnaCheckout", viewModel);
        }
    }
}