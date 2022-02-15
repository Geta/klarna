using System.Linq;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Web;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Routing;
using Foundation.Features.Checkout.Services;
using Foundation.Features.MyAccount.AddressBook;
using Foundation.Infrastructure.Commerce.Customer.Services;
using Klarna.Checkout;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders.Managers;
//using Foundation.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Features.MyAccount.OrderConfirmation
{
    public class OrderConfirmationController : OrderConfirmationControllerBase<OrderConfirmationPage>
    {
        //private readonly ICampaignService _campaignService;
        private readonly IContextModeResolver _contextModeResolver;
        private readonly IMarketService _marketService;
        private readonly IKlarnaCheckoutService _klarnaCheckoutService;
        public OrderConfirmationController(
            //ICampaignService campaignService,
            IConfirmationService confirmationService,
            IAddressBookService addressBookService,
            IOrderGroupCalculator orderGroupCalculator,
            UrlResolver urlResolver, ICustomerService customerService,
            IContextModeResolver contextModeResolver, IMarketService marketService,
            IKlarnaCheckoutService klarnaCheckoutService) :
            base(confirmationService, addressBookService, orderGroupCalculator, urlResolver, customerService)
        {
            //_campaignService = campaignService;
            _contextModeResolver = contextModeResolver;
            _marketService = marketService;
            _klarnaCheckoutService = klarnaCheckoutService;
        }

        public async Task<ActionResult> Index(OrderConfirmationPage currentPage, string notificationMessage, int? orderNumber)
        {
            IPurchaseOrder order = null;
            if (_contextModeResolver.CurrentMode.EditOrPreview())
            {
                order = _confirmationService.CreateFakePurchaseOrder();
            }
            else if (orderNumber.HasValue)
            {
                order = _confirmationService.GetOrder(orderNumber.Value);
            }

            if (order != null && order.CustomerId == _customerService.CurrentContactId)
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
                    var klarnaOrder = await _klarnaCheckoutService.GetOrder(order.Properties[Klarna.Common.Constants.KlarnaOrderIdField].ToString(), market).ConfigureAwait(false);
                    viewModel.KlarnaCheckoutHtmlSnippet = klarnaOrder.HtmlSnippet;
                    viewModel.IsKlarnaCheckout = true;
                }

                //_campaignService.UpdateLastOrderDate();
                //_campaignService.UpdatePoint(decimal.ToInt16(viewModel.SubTotal.Amount));

                return View(viewModel);
            }

            return Redirect(Url.ContentUrl(ContentReference.StartPage));
        }
    }
}