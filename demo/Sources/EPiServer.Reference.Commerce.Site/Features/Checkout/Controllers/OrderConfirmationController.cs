using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Mvc.Html;
using Mediachase.Commerce.Markets;
using System.Threading.Tasks;
using System.Web.Mvc;
using EPiServer.Globalization;
using Klarna.Checkout;
using Mediachase.Commerce.Orders.Managers;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public class OrderConfirmationController : OrderConfirmationControllerBase<OrderConfirmationPage>
    {
        private readonly IMarketService _marketService;
        private readonly IRecommendationService _recommendationService;
        private readonly IKlarnaCheckoutService _klarnaCheckoutService;

        public OrderConfirmationController(
            ConfirmationService confirmationService,
            AddressBookService addressBookService,
            CustomerContextFacade customerContextFacade,
            IOrderGroupCalculator orderGroupCalculator,
            IMarketService marketService,
            IRecommendationService recommendationService,
            IKlarnaCheckoutService klarnaCheckoutService)
            : base(confirmationService, addressBookService, customerContextFacade, orderGroupCalculator, marketService)
        {
            _marketService = marketService;
            _recommendationService = recommendationService;
            _klarnaCheckoutService = klarnaCheckoutService;
        }

        [HttpGet]
        public async Task<ActionResult> Index(OrderConfirmationPage currentPage, string notificationMessage, string orderNumber, string trackingNumber)
        {
            IPurchaseOrder order = null;
            int orderId;
            trackingNumber = string.IsNullOrEmpty(trackingNumber) ? orderNumber : trackingNumber;
            if (PageEditing.PageIsInEditMode)
            {
                order = ConfirmationService.CreateFakePurchaseOrder();
            }
            else if (int.TryParse(orderNumber, out orderId))
            {
                order = ConfirmationService.GetOrder(orderId);
            }
            else if (!string.IsNullOrEmpty(trackingNumber))
            {
                order = ConfirmationService.GetByTrackingNumber(trackingNumber);
            }

            if (order != null)
            {
                await _recommendationService.TrackOrderAsync(HttpContext, order);
            }

            if (order != null && order.CustomerId == CustomerContext.CurrentContactId)
            {
                var viewModel = CreateViewModel(currentPage, order);
                viewModel.NotificationMessage = notificationMessage;

                var paymentMethod = PaymentManager
                    .GetPaymentMethodBySystemName(Constants.KlarnaCheckoutSystemKeyword,
                        ContentLanguage.PreferredCulture.Name)
                    .PaymentMethod.FirstOrDefault();

                if (paymentMethod != null &&
                    order.GetFirstForm().Payments.Any(x => x.PaymentMethodId == paymentMethod.PaymentMethodId &&
                                                           !string.IsNullOrEmpty(order.Properties[Klarna.Common.Constants.KlarnaOrderIdField]?.ToString())))
                {
                    var market = _marketService.GetMarket(order.MarketId);
                    var klarnaOrder =
                        _klarnaCheckoutService.GetOrder(
                            order.Properties[Klarna.Common.Constants.KlarnaOrderIdField].ToString(), market);
                    viewModel.KlarnaCheckoutHtmlSnippet = klarnaOrder.HtmlSnippet;
                    viewModel.IsKlarnaCheckout = true;
                }

                return View(viewModel);
            }

            return Redirect(Url.ContentUrl(ContentReference.StartPage));
        }
    }
}