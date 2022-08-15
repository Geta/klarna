using System.Threading.Tasks;
using EPiServer;
using EPiServer.Commerce.Order;
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
        private readonly IOrderRepository _orderRepository;
        private readonly IKlarnaCheckoutService _klarnaCheckoutService;
        private readonly CheckoutService _checkoutService;

        public KlarnaCheckoutPageController(ICartService cartService, CheckoutViewModelFactory checkoutViewModelFactory, ISettingsService settingsService, IContentLoader contentLoader, IOrderRepository orderRepository, IKlarnaCheckoutService klarnaCheckoutService, CheckoutService checkoutService)
        {
            _cartService = cartService;
            _checkoutViewModelFactory = checkoutViewModelFactory;
            _contentLoader = contentLoader;
            _settingsService = settingsService;
            _orderRepository = orderRepository;
            _klarnaCheckoutService = klarnaCheckoutService;
            _checkoutService = checkoutService;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateShippingMethods([FromForm] CheckoutViewModel viewModel)
        {
            var cartWithValidation = _cartService.LoadCart(_cartService.DefaultCartName, true);

            _checkoutService.UpdateShippingMethods(cartWithValidation.Cart, viewModel.Shipments);
            _checkoutService.ApplyDiscounts(cartWithValidation.Cart);
            _orderRepository.Save(cartWithValidation.Cart);

            await _klarnaCheckoutService.CreateOrUpdateOrder(cartWithValidation.Cart);

            return Ok();
        }
    }
}