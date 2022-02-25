using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Commerce.Order;
using EPiServer.Web.Mvc;
using Foundation.Features.Checkout.Payments;
using Foundation.Features.Checkout.Services;
using Foundation.Features.Checkout.ViewModels;
using Foundation.Features.MyAccount.AddressBook;
using Foundation.Features.Settings;
using Foundation.Infrastructure.Cms.Settings;
using Foundation.Infrastructure.Commerce;
using Foundation.Infrastructure.Commerce.Customer.Services;
using Klarna.Payments;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Features.Checkout
{
    public class KlarnaPaymentsPageController : PageController<KlarnaPaymentsPage>
    {
        private readonly ICartService _cartService;
        private readonly CheckoutViewModelFactory _checkoutViewModelFactory;
        private readonly OrderSummaryViewModelFactory _orderSummaryViewModelFactory;
        private readonly CheckoutService _checkoutService;
        private readonly IOrderRepository _orderRepository;
        private readonly IKlarnaPaymentsService _klarnaPaymentsService;
        private readonly IAddressBookService _addressBookService;
        private readonly ICustomerService _customerService;
        private readonly ISettingsService _settingsService;
        private readonly IContentLoader _contentLoader;
        private readonly IOrderGroupFactory _orderGroupFactory;

        private CartWithValidationIssues _cart;

        public KlarnaPaymentsPageController(ICartService cartService, CheckoutViewModelFactory checkoutViewModelFactory, OrderSummaryViewModelFactory orderSummaryViewModelFactory, CheckoutService checkoutService, IOrderRepository orderRepository, IKlarnaPaymentsService klarnaPaymentsService, IAddressBookService addressBookService, ICustomerService customerService, ISettingsService settingsService, IContentLoader contentLoader, IOrderGroupFactory orderGroupFactory)
        {
            _cartService = cartService;
            _checkoutViewModelFactory = checkoutViewModelFactory;
            _orderSummaryViewModelFactory = orderSummaryViewModelFactory;
            _checkoutService = checkoutService;
            _orderRepository = orderRepository;
            _klarnaPaymentsService = klarnaPaymentsService;
            _addressBookService = addressBookService;
            _customerService = customerService;
            _settingsService = settingsService;
            _contentLoader = contentLoader;
            _orderGroupFactory = orderGroupFactory;
        }

        public IActionResult Index(KlarnaPaymentsPage currentPage)
        {
            if (currentPage == null)
            {
                currentPage = _contentLoader.Get<KlarnaPaymentsPage>(_settingsService.GetSiteSettings<ReferencePageSettings>().KlarnaPaymentsPage);
            }

            var viewModel = _checkoutViewModelFactory.CreateCheckoutViewModel(CartWithValidationIssues.Cart, currentPage, Constants.KlarnaPaymentSystemKeyword.GetPaymentMethod());
            viewModel.BillingAddressType = 2; // same as shipping address by default

            ((KlarnaPaymentsPaymentOption)viewModel.Payment).Initialize();

            viewModel.OrderSummary = _orderSummaryViewModelFactory.CreateOrderSummaryViewModel(CartWithValidationIssues.Cart);

            return View("KlarnaPayments", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutPage currentPage, [FromForm] CheckoutViewModel checkoutViewModel)
        {
            ModelState.Clear();

            checkoutViewModel.OrderSummary = _orderSummaryViewModelFactory.CreateOrderSummaryViewModel(CartWithValidationIssues.Cart);

            checkoutViewModel.Payment = Constants.KlarnaPaymentSystemKeyword.GetPaymentMethod();

            // Clean up payments in cart on payment provider site.
            foreach (var form in CartWithValidationIssues.Cart.Forms)
            {
                form.Payments.Clear();
            }

            var payment = checkoutViewModel.Payment.CreatePayment(checkoutViewModel.OrderSummary.PaymentTotal, CartWithValidationIssues.Cart);

            CartWithValidationIssues.Cart.AddPayment(payment, _orderGroupFactory);

            payment.Properties[Constants.AuthorizationTokenPaymentField] = checkoutViewModel.AuthorizationToken;

            // store the shipment indexes and billing address properties if they are invalid when run TryValidateModel
            // format: key = Shipment | Billing
            var errorTypes = new List<KeyValuePair<string, int>>();

            // shipping information
            UpdateShipmentAddress(checkoutViewModel, errorTypes);

            // billing address
            UpdatePaymentAddress(checkoutViewModel, errorTypes);

            _orderRepository.Save(CartWithValidationIssues.Cart);

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            try
            {
                var purchaseOrder = _checkoutService.PlaceOrder(CartWithValidationIssues.Cart, ModelState, checkoutViewModel);
                if (purchaseOrder == null)
                {
                    TempData[Constant.ErrorMessages] = "No payment was processed";
                    return RedirectToAction("Index");
                }

                if (checkoutViewModel.BillingAddressType == 0 && checkoutViewModel.BillingAddress != null)
                {
                    _addressBookService.Save(checkoutViewModel.BillingAddress);
                }

                if (checkoutViewModel.Shipments != null)
                {
                    foreach (var shipment in checkoutViewModel.Shipments)
                    {
                        if (shipment.ShippingAddressType == 0 && shipment.ShippingMethodId != _cartService.InStorePickupInfoModel.MethodId)
                        {
                            _addressBookService.Save(shipment.Address);
                        }
                    }
                }

                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    var contact = _customerService.GetCurrentContact().Contact;
                    var organization = contact.ContactOrganization;
                    if (organization != null)
                    {
                        purchaseOrder.Properties[Constant.Customer.CustomerFullName] = contact.FullName;
                        purchaseOrder.Properties[Constant.Customer.CustomerEmailAddress] = contact.Email;
                        purchaseOrder.Properties[Constant.Customer.CurrentCustomerOrganization] = organization.Name;
                        _orderRepository.Save(purchaseOrder);
                    }
                }

                var result = _klarnaPaymentsService.Complete(purchaseOrder);
                if (result.IsRedirect)
                {
                    return Redirect(result.RedirectUrl);
                }

                checkoutViewModel.CurrentContent = currentPage;
                var confirmationSentSuccessfully = await _checkoutService.SendConfirmation(checkoutViewModel, purchaseOrder);

                return Redirect(_checkoutService.BuildRedirectionUrl(checkoutViewModel, purchaseOrder, confirmationSentSuccessfully));
            }
            catch (Exception e)
            {
                TempData[Constant.ErrorMessages] = e.Message;
                return RedirectToAction("Index");
            }
        }

        public void UpdatePaymentAddress(CheckoutViewModel viewModel, List<KeyValuePair<string, int>> errorTypes)
        {
            if (viewModel.BillingAddressType == 1)
            {
                if (string.IsNullOrEmpty(viewModel.BillingAddress.AddressId))
                {
                    ModelState.AddModelError("BillingAddress.AddressId", "Address is required.");
                    return;
                }

                _addressBookService.LoadAddress(viewModel.BillingAddress);
            }
            else if (viewModel.BillingAddressType == 2)
            {
                viewModel.BillingAddress = viewModel.Shipments.FirstOrDefault()?.Address;
                if (viewModel.BillingAddress == null)
                {
                    ModelState.AddModelError("BillingAddress.AddressId", "Shipping address is required.");
                    return;
                }
            }
            else
            {
                var addressName = viewModel.BillingAddress.FirstName + " " + viewModel.BillingAddress.LastName;
                viewModel.BillingAddress.AddressId = null;
                viewModel.BillingAddress.Name = addressName + " " + DateTime.Now;

                if (!TryValidateModel(viewModel.BillingAddress, "BillingAddress"))
                {
                    errorTypes.Add(new KeyValuePair<string, int>("Billing", 1));
                }
            }

            foreach (var payment in CartWithValidationIssues.Cart.GetFirstForm().Payments)
            {
                payment.BillingAddress = _addressBookService.ConvertToAddress(viewModel.BillingAddress, CartWithValidationIssues.Cart);
            }
        }

        public void UpdateShipmentAddress(CheckoutViewModel checkoutViewModel, List<KeyValuePair<string, int>> errorTypes)
        {
            var content = _settingsService.GetSiteSettings<ReferencePageSettings>().CheckoutPage;
            var checkoutPage = _contentLoader.Get<CheckoutPage>(content);
            var viewModel = _checkoutViewModelFactory.CreateCheckoutViewModel(CartWithValidationIssues.Cart, checkoutPage);
            if (!checkoutViewModel.UseShippingingAddressForBilling)
            {
                for (var i = 0; i < checkoutViewModel.Shipments.Count; i++)
                {
                    if (checkoutViewModel.Shipments[i].ShippingAddressType == 0)
                    {
                        var addressName = checkoutViewModel.Shipments[i].Address.FirstName + " " + checkoutViewModel.Shipments[i].Address.LastName;
                        checkoutViewModel.Shipments[i].Address.AddressId = null;
                        checkoutViewModel.Shipments[i].Address.Name = addressName + " " + DateTime.Now;
                        viewModel.Shipments[i].Address = checkoutViewModel.Shipments[i].Address;

                        if (!TryValidateModel(checkoutViewModel.Shipments[i].Address, "Shipments[" + i + "].Address"))
                        {
                            errorTypes.Add(new KeyValuePair<string, int>("Shipment", i));
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(checkoutViewModel.Shipments[i].Address.AddressId))
                        {
                            viewModel.Shipments[i].ShippingAddressType
                                = 1;
                            ModelState.AddModelError("Shipments[" + i + "].Address.AddressId", "Address is required.");
                        }

                        _addressBookService.LoadAddress(checkoutViewModel.Shipments[i].Address);
                        viewModel.Shipments[i].Address = checkoutViewModel.Shipments[i].Address;
                    }
                }
            }

            _checkoutService.UpdateShippingAddresses(CartWithValidationIssues.Cart, viewModel);
        }

        private CartWithValidationIssues CartWithValidationIssues => _cart ??= _cartService.LoadCart(_cartService.DefaultCartName, true);
    }
}