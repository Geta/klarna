using System;
using System.Collections.Generic;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Recommendations.Tracking;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Routing;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using Klarna.Payments;
using Klarna.Payments.Extensions;
using Klarna.Payments.Helpers;
using Klarna.Payments.Models;
using Mediachase.Commerce.Customers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public class CheckoutController : PageController<CheckoutPage>
    {
        private readonly ICurrencyService _currencyService;
        private readonly ControllerExceptionHandler _controllerExceptionHandler;
        private readonly CheckoutViewModelFactory _checkoutViewModelFactory;
        private readonly OrderSummaryViewModelFactory _orderSummaryViewModelFactory;
        private readonly IOrderRepository _orderRepository;
        private readonly ICartService _cartService;
        private readonly IRecommendationService _recommendationService;
        private ICart _cart;
        private readonly CheckoutService _checkoutService;
        private readonly IKlarnaService _klarnaService;

        public CheckoutController(
            ICurrencyService currencyService,
            ControllerExceptionHandler controllerExceptionHandler,
            IOrderRepository orderRepository,
            CheckoutViewModelFactory checkoutViewModelFactory,
            ICartService cartService,
            OrderSummaryViewModelFactory orderSummaryViewModelFactory,
            IRecommendationService recommendationService,
            CheckoutService checkoutService,
            IKlarnaService klarnaService)
        {
            _currencyService = currencyService;
            _controllerExceptionHandler = controllerExceptionHandler;
            _orderRepository = orderRepository;
            _checkoutViewModelFactory = checkoutViewModelFactory;
            _cartService = cartService;
            _orderSummaryViewModelFactory = orderSummaryViewModelFactory;
            _recommendationService = recommendationService;
            _checkoutService = checkoutService;
            _klarnaService = klarnaService;
        }

        private Session GetSessionRequest(Session sessionRequest)
        {
            if (_klarnaService.Configuration.IsCustomerPreAssessmentEnabled)
            {
                sessionRequest.Customer = new Customer
                {
                    DateOfBirth = "1980-01-01",
                    Gender = "Male",
                    LastFourSsn = "1234"
                };
            }
            sessionRequest.MerchantReference2 = "12345";

            if (_klarnaService.Configuration.UseAttachments)
            {
                var converter = new IsoDateTimeConverter
                {
                    DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"
                };

                var customerAccountInfos = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        { "unique_account_identifier",  "Test Testperson" },
                        { "account_registration_date", DateTime.Now },
                        { "account_last_modified", DateTime.Now }
                    }
                };

                var emd = new Dictionary<string, object>
                {
                    { "customer_account_info", customerAccountInfos}
                };

                sessionRequest.Attachment = new Attachment
                {
                    ContentType = "application/vnd.klarna.internal.emd-v2+json",
                    Body = JsonConvert.SerializeObject(emd, converter)
                };
            }
            return sessionRequest;
        }

        [HttpGet]
        [OutputCache(Duration = 0, NoStore = true)]
        [Tracking(TrackingType.Checkout)]
        public async Task<ActionResult> Index(CheckoutPage currentPage)
        {
            if (CartIsNullOrEmpty())
            {
                return View("EmptyCart");
            }

            var viewModel = CreateCheckoutViewModel(currentPage);

            Cart.Currency = _currencyService.GetCurrentCurrency();
            
            if (User.Identity.IsAuthenticated)
            {
                _checkoutService.UpdateShippingAddresses(Cart, viewModel);
            }

            _checkoutService.UpdateShippingMethods(Cart, viewModel.Shipments);
            _checkoutService.ApplyDiscounts(Cart);
            _orderRepository.Save(Cart);

            var sessionRequest = _klarnaService.GetSessionRequest(Cart);

            sessionRequest = GetSessionRequest(sessionRequest);

            await _klarnaService.CreateOrUpdateSession(sessionRequest, Cart);

            return View(viewModel.ViewName, viewModel);
        }

        [HttpGet]
        public ActionResult SingleShipment(CheckoutPage currentPage)
        {
            if (!CartIsNullOrEmpty())
            {
                _cartService.MergeShipments(Cart);
                _orderRepository.Save(Cart);
            }

            return RedirectToAction("Index", new { node = currentPage.ContentLink });
        }

        [HttpPost]
        [AllowDBWrite]
        public async Task<ActionResult> Update(CheckoutPage currentPage, UpdateShippingMethodViewModel shipmentViewModel, IPaymentMethodViewModel<PaymentMethodBase> paymentViewModel, CheckoutViewModel inputModel)
        {
            ModelState.Clear();

            _checkoutService.UpdateShippingMethods(Cart, shipmentViewModel.Shipments);
            _checkoutService.ApplyDiscounts(Cart);
            _orderRepository.Save(Cart);

            var viewModel = CreateCheckoutViewModel(currentPage, paymentViewModel);

            var sessionRequest = _klarnaService.GetSessionRequest(Cart);

            if (paymentViewModel.SystemName.Equals(Constants.KlarnaPaymentSystemKeyword, StringComparison.InvariantCultureIgnoreCase))
            {
                var shipment = shipmentViewModel.Shipments.FirstOrDefault();
                if (User.Identity.IsAuthenticated)
                {
                    if (inputModel.BillingAddress != null && !string.IsNullOrEmpty(inputModel.BillingAddress.AddressId))
                    {
                        var address =
                            CustomerContext.Current.CurrentContact.ContactAddresses.FirstOrDefault(
                                x => x.Name == inputModel.BillingAddress.AddressId)?.ToAddress();
                        if (address != null)
                        {
                            sessionRequest.BillingAddress = address;
                        }
                    }
                    if (shipment != null && shipment.Address != null &&
                        !string.IsNullOrEmpty(shipment.Address.AddressId))
                    {
                        var address =
                            CustomerContext.Current.CurrentContact.ContactAddresses.FirstOrDefault(
                                x => x.Name == shipment.Address.AddressId)?.ToAddress();
                        if (address != null)
                        {
                            sessionRequest.ShippingAddress = address;
                        }
                    }
                }
                else
                {
                    var address = new Address();
                    address.GivenName = inputModel.BillingAddress.FirstName;
                    address.FamilyName = inputModel.BillingAddress.LastName;
                    address.StreetAddress = inputModel.BillingAddress.Line1;
                    address.StreetAddress2 = inputModel.BillingAddress.Line2;
                    address.PostalCode = inputModel.BillingAddress.PostalCode;
                    address.City = inputModel.BillingAddress.City;
                    address.Region = inputModel.BillingAddress.CountryRegion.Region;
                    address.Country = CountryCodeHelper.GetTwoLetterCountryCode(inputModel.BillingAddress.CountryCode);
                    address.Email = inputModel.BillingAddress.Email;
                    address.Phone = inputModel.BillingAddress.DaytimePhoneNumber;

                    sessionRequest.BillingAddress = address;

                    if (shipment != null && shipment.Address != null)
                    {
                        var shipmentAddress = new Address();
                        shipmentAddress.GivenName = shipment.Address.FirstName;
                        shipmentAddress.FamilyName = shipment.Address.LastName;
                        shipmentAddress.StreetAddress = shipment.Address.Line1;
                        shipmentAddress.StreetAddress2 = shipment.Address.Line2;
                        shipmentAddress.PostalCode = shipment.Address.PostalCode;
                        shipmentAddress.City = shipment.Address.City;
                        shipmentAddress.Region = shipment.Address.CountryRegion.Region;
                        shipmentAddress.Country = CountryCodeHelper.GetTwoLetterCountryCode(shipment.Address.CountryCode);
                        shipmentAddress.Email = shipment.Address.Email;
                        shipmentAddress.Phone = shipment.Address.DaytimePhoneNumber;

                        sessionRequest.ShippingAddress = address;
                    }
                }
            }
            await _klarnaService.CreateOrUpdateSession(sessionRequest, Cart);

            return PartialView("Partial", viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult ChangeAddress(UpdateAddressViewModel addressViewModel)
        {
            ModelState.Clear();
            var viewModel = CreateCheckoutViewModel(addressViewModel.CurrentPage);
            _checkoutService.CheckoutAddressHandling.ChangeAddress(viewModel, addressViewModel);

            if (User.Identity.IsAuthenticated)
            {
                _checkoutService.UpdateShippingAddresses(Cart, viewModel);
            }
            
            _orderRepository.Save(Cart);

            var addressViewName = addressViewModel.ShippingAddressIndex > -1 ? "SingleShippingAddress" : "BillingAddress";

            return PartialView(addressViewName, viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult OrderSummary()
        {
            var viewModel = _orderSummaryViewModelFactory.CreateOrderSummaryViewModel(Cart);
            return PartialView(viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult AddCouponCode(CheckoutPage currentPage, string couponCode)
        {
            if (_cartService.AddCouponCode(Cart, couponCode))
            {
                _orderRepository.Save(Cart);
            }
            var viewModel = CreateCheckoutViewModel(currentPage);
            return View(viewModel.ViewName, viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult RemoveCouponCode(CheckoutPage currentPage, string couponCode)
        {
            _cartService.RemoveCouponCode(Cart, couponCode);
            _orderRepository.Save(Cart);
            var viewModel = CreateCheckoutViewModel(currentPage);
            return View(viewModel.ViewName, viewModel);
        }
        
        [HttpPost]
        [AllowDBWrite]
        public ActionResult Purchase(CheckoutViewModel viewModel, IPaymentMethodViewModel<PaymentMethodBase> paymentViewModel)
        {
            if (CartIsNullOrEmpty())
            {
                return Redirect(Url.ContentUrl(ContentReference.StartPage));
            }

            // Since the payment property is marked with an exclude binding attribute in the CheckoutViewModel
            // it needs to be manually re-added again.
            viewModel.Payment = paymentViewModel;
            
            if (User.Identity.IsAuthenticated)
            {
                _checkoutService.CheckoutAddressHandling.UpdateAuthenticatedUserAddresses(viewModel);

                var validation = _checkoutService.AuthenticatedPurchaseValidation;

                if (!validation.ValidateModel(ModelState, viewModel) ||
                    !validation.ValidateOrderOperation(ModelState, _cartService.ValidateCart(Cart)) ||
                    !validation.ValidateOrderOperation(ModelState, _cartService.RequestInventory(Cart)))
                {
                    return View(viewModel);
                }
            }
            else
            {
                _checkoutService.CheckoutAddressHandling.UpdateAnonymousUserAddresses(viewModel);

                var validation = _checkoutService.AnonymousPurchaseValidation;
              
                if (!validation.ValidateModel(ModelState, viewModel) ||
                    !validation.ValidateOrderOperation(ModelState, _cartService.ValidateCart(Cart)) ||
                    !validation.ValidateOrderOperation(ModelState, _cartService.RequestInventory(Cart)))
                {
                    return View(viewModel);
                }
            }

            _checkoutService.UpdateShippingAddresses(Cart, viewModel);
            _checkoutService.CreateAndAddPaymentToCart(Cart, viewModel);

            var purchaseOrder = _checkoutService.PlaceOrder(Cart, ModelState, viewModel);
            if (purchaseOrder == null)
            {
                return View(viewModel);
            }

            _klarnaService.RedirectToConfirmationUrl(purchaseOrder);
            
            var confirmationSentSuccessfully = _checkoutService.SendConfirmation(viewModel, purchaseOrder);
          
            _recommendationService.SendOrderTracking(HttpContext, purchaseOrder);

            return Redirect(_checkoutService.BuildRedirectionUrl(viewModel, purchaseOrder, confirmationSentSuccessfully));
        }

        public ActionResult OnPurchaseException(ExceptionContext filterContext)
        {
            var currentPage = filterContext.RequestContext.GetRoutedData<CheckoutPage>();
            if (currentPage == null)
            {
                return new EmptyResult();
            }

            var viewModel = CreateCheckoutViewModel(currentPage);
            ModelState.AddModelError("Purchase", filterContext.Exception.Message);

            return View(viewModel.ViewName, viewModel);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            _controllerExceptionHandler.HandleRequestValidationException(filterContext, "purchase", OnPurchaseException);
        }

        private ViewResult View(CheckoutViewModel checkoutViewModel)
        {
            return View(checkoutViewModel.ViewName, CreateCheckoutViewModel(checkoutViewModel.CurrentPage, checkoutViewModel.Payment));
        }

        private CheckoutViewModel CreateCheckoutViewModel(CheckoutPage currentPage, IPaymentMethodViewModel<PaymentMethodBase> paymentViewModel = null)
        {
            return _checkoutViewModelFactory.CreateCheckoutViewModel(Cart, currentPage, paymentViewModel);
        }

        private ICart Cart
        {
            get { return _cart ?? (_cart = _cartService.LoadCart(_cartService.DefaultCartName)); }
        }

        private bool CartIsNullOrEmpty()
        {
            return Cart == null || !Cart.GetAllLineItems().Any();
        }
    }
}
