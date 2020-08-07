using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.Globalization;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Klarna.Checkout;
using Klarna.Common.Extensions;
using Klarna.Payments.Models;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Exceptions;
using Mediachase.Commerce.Orders.Managers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Klarna.Common.Models;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Services
{
    public class CheckoutService
    {
        private readonly IAddressBookService _addressBookService;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IOrderRepository _orderRepository;
        private readonly IContentRepository _contentRepository;
        private readonly CustomerContextFacade _customerContext;
        private readonly LocalizationService _localizationService;
        private readonly IMailService _mailService;
        private readonly IContentLoader _contentLoader;
        private readonly IKlarnaCheckoutService _klarnaCheckoutService;
        private readonly ILogger _log = LogManager.GetLogger(typeof(CheckoutService));
        private readonly ICartService _cartService;

        public AuthenticatedPurchaseValidation AuthenticatedPurchaseValidation { get; }
        public AnonymousPurchaseValidation AnonymousPurchaseValidation { get; }
        public CheckoutAddressHandling CheckoutAddressHandling { get; }

        public CheckoutService(
            IAddressBookService addressBookService,
            IOrderGroupFactory orderGroupFactory,
            IOrderGroupCalculator orderGroupCalculator,
            IPaymentProcessor paymentProcessor,
            IOrderRepository orderRepository,
            IContentRepository contentRepository,
            CustomerContextFacade customerContext,
            LocalizationService localizationService,
            IMailService mailService,
            IContentLoader contentLoader,
            IKlarnaCheckoutService klarnaCheckoutService,
            ICartService cartService)
        {
            _addressBookService = addressBookService;
            _orderGroupFactory = orderGroupFactory;
            _orderGroupCalculator = orderGroupCalculator;
            _paymentProcessor = paymentProcessor;
            _orderRepository = orderRepository;
            _contentRepository = contentRepository;
            _customerContext = customerContext;
            _localizationService = localizationService;
            _mailService = mailService;
            _contentLoader = contentLoader;
            _klarnaCheckoutService = klarnaCheckoutService;
            _cartService = cartService;

            AuthenticatedPurchaseValidation = new AuthenticatedPurchaseValidation(_localizationService);
            AnonymousPurchaseValidation = new AnonymousPurchaseValidation(_localizationService);
            CheckoutAddressHandling = new CheckoutAddressHandling(_addressBookService);
        }

        public virtual void UpdateShippingMethods(ICart cart, IList<ShipmentViewModel> shipmentViewModels)
        {
            var index = 0;
            foreach (var shipment in cart.GetFirstForm().Shipments)
            {
                shipment.ShippingMethodId = shipmentViewModels[index++].ShippingMethodId;
            }
        }

        public virtual void UpdateShippingAddresses(ICart cart, CheckoutViewModel viewModel)
        {
            if (viewModel.UseBillingAddressForShipment)
            {
                cart.GetFirstShipment().ShippingAddress = _addressBookService.ConvertToAddress(viewModel.BillingAddress, cart);
            }
            else
            {
                var shipments = cart.GetFirstForm().Shipments;
                for (var index = 0; index < shipments.Count; index++)
                {
                    shipments.ElementAt(index).ShippingAddress = _addressBookService.ConvertToAddress(viewModel.Shipments[index].Address, cart);
                }
            }
        }

        public virtual void CreateAndAddPaymentToCart(ICart cart, CheckoutViewModel viewModel)
        {
            // Clean up payments in cart on payment provider site.
            foreach (var form in cart.Forms)
            {
                form.Payments.Clear();
            }

            var total = cart.GetTotal(_orderGroupCalculator);
            var payment = viewModel.Payment.CreatePayment(total.Amount, cart);
            if (payment.PaymentMethodName.Equals(Klarna.Payments.Constants.KlarnaPaymentSystemKeyword))
            {
                payment.Properties[Klarna.Payments.Constants.AuthorizationTokenPaymentField] = viewModel.AuthorizationToken;
            }

            cart.AddPayment(payment, _orderGroupFactory);
            payment.BillingAddress = _addressBookService.ConvertToAddress(viewModel.BillingAddress, cart);
        }

        public virtual IPurchaseOrder PlaceOrder(ICart cart, ModelStateDictionary modelState, CheckoutViewModel checkoutViewModel)
        {
            try
            {
                var paymentProcessingResults = cart.ProcessPayments(_paymentProcessor, _orderGroupCalculator).ToList();

                if (paymentProcessingResults.Any(r => !r.IsSuccessful))
                {
                    modelState.AddModelError("", _localizationService.GetString("/Checkout/Payment/Errors/ProcessingPaymentFailure") + string.Join(", ", paymentProcessingResults.Select(p => p.Message)));
                    return null;
                }

                var redirectPayment = paymentProcessingResults.FirstOrDefault(r => !string.IsNullOrEmpty(r.RedirectUrl));
                if (redirectPayment != null)
                {
                    checkoutViewModel.RedirectUrl = redirectPayment.RedirectUrl;
                    return null;
                }

                var processedPayments = cart.GetFirstForm().Payments.Where(x => x.Status.Equals(PaymentStatus.Processed.ToString())).ToList();
                if (!processedPayments.Any())
                {
                    // Return null in case there is no payment was processed.
                    return null;
                }

                var totalProcessedAmount = processedPayments.Sum(x => x.Amount);
                if (totalProcessedAmount != cart.GetTotal(_orderGroupCalculator).Amount)
                {
                    throw new InvalidOperationException("Wrong amount");
                }

                PurchaseValidation validation;
                if (checkoutViewModel.IsAuthenticated)
                {
                    validation = AuthenticatedPurchaseValidation;
                }
                else
                {
                    validation = AnonymousPurchaseValidation;
                }

                if (!validation.ValidateOrderOperation(modelState,  _cartService.RequestInventory(cart)))
                {
                    return null;
                }

                var orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
                var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
                _orderRepository.Delete(cart.OrderLink);

                return purchaseOrder;
            }
            catch (PaymentException ex)
            {
                modelState.AddModelError("", _localizationService.GetString("/Checkout/Payment/Errors/ProcessingPaymentFailure") + ex.Message);
            }
            return null;
        }

        public virtual bool SendConfirmation(CheckoutViewModel viewModel, IPurchaseOrder purchaseOrder)
        {
            var queryCollection = new NameValueCollection
            {
                {"contactId", _customerContext.CurrentContactId.ToString()},
                {"orderNumber", purchaseOrder.OrderLink.OrderGroupId.ToString(CultureInfo.CurrentCulture)}
            };

            var confirmationPage = _contentRepository.GetChildren<OrderConfirmationPage>(viewModel.CurrentPage.ContentLink).First();

            try
            {
                _mailService.Send(_contentLoader.GetStartPage().OrderConfirmationMail, queryCollection, viewModel.BillingAddress.Email, confirmationPage.Language.Name);
            }
            catch (Exception e)
            {
                _log.Warning($"Unable to send purchase receipt to '{viewModel.BillingAddress.Email}'.", e);
                return false;
            }
            return true;
        }

        public virtual string BuildRedirectionUrl(CheckoutViewModel checkoutViewModel, IPurchaseOrder purchaseOrder, bool confirmationSentSuccessfully)
        {
            var queryCollection = new NameValueCollection
            {
                {"contactId", _customerContext.CurrentContactId.ToString()},
                {"orderNumber", purchaseOrder.OrderLink.OrderGroupId.ToString(CultureInfo.CurrentCulture)}
            };

            if (!confirmationSentSuccessfully)
            {
                queryCollection.Add("notificationMessage", string.Format(_localizationService.GetString("/OrderConfirmationMail/ErrorMessages/SmtpFailure"), checkoutViewModel.BillingAddress.Email));
            }

            var confirmationPage = _contentRepository.GetChildren<OrderConfirmationPage>(checkoutViewModel.CurrentPage.ContentLink).First();

            return new UrlBuilder(confirmationPage.LinkURL) {QueryCollection = queryCollection}.ToString();
        }

        public virtual string BuildRedirectionUrl(IPurchaseOrder purchaseOrder)
        {
            var queryCollection = new NameValueCollection
            {
                {"contactId", _customerContext.CurrentContactId.ToString()},
                {"orderNumber", purchaseOrder.OrderLink.OrderGroupId.ToString(CultureInfo.CurrentCulture)}
            };

            var confirmationPage = _contentRepository.GetFirstChild<OrderConfirmationPage>(_contentLoader.GetStartPage().CheckoutPage);

            return new UrlBuilder(confirmationPage.LinkURL) { QueryCollection = queryCollection }.ToString();
        }

        public IPurchaseOrder CreatePurchaseOrderForKlarna(string klarnaOrderId, CheckoutOrder order, ICart cart)
        {
            var paymentRow = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaCheckoutSystemKeyword, ContentLanguage.PreferredCulture.Name).PaymentMethod.FirstOrDefault();

            var payment = cart.CreatePayment(_orderGroupFactory);
            payment.PaymentType = PaymentType.Other;
            payment.PaymentMethodId = paymentRow.PaymentMethodId;
            payment.PaymentMethodName = Constants.KlarnaCheckoutSystemKeyword;
            payment.Amount = cart.GetTotal(_orderGroupCalculator).Amount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = TransactionType.Authorization.ToString();

            cart.AddPayment(payment, _orderGroupFactory);

            var billingAddress = new AddressModel
            {
                Name = $"{order.BillingCheckoutAddress.StreetAddress}{order.BillingCheckoutAddress.StreetAddress2}{order.BillingCheckoutAddress.City}",
                FirstName = order.BillingCheckoutAddress.GivenName,
                LastName = order.BillingCheckoutAddress.FamilyName,
                Email = order.BillingCheckoutAddress.Email,
                DaytimePhoneNumber = order.BillingCheckoutAddress.Phone,
                Line1 = order.BillingCheckoutAddress.StreetAddress,
                Line2 = order.BillingCheckoutAddress.StreetAddress2,
                PostalCode = order.BillingCheckoutAddress.PostalCode,
                City = order.BillingCheckoutAddress.City,
                CountryName = order.BillingCheckoutAddress.Country
            };

            payment.BillingAddress = _addressBookService.ConvertToAddress(billingAddress, cart);

            cart.ProcessPayments(_paymentProcessor, _orderGroupCalculator);

            var totalProcessedAmount = cart.GetFirstForm().Payments.Where(x => x.Status.Equals(PaymentStatus.Processed.ToString())).Sum(x => x.Amount);
            if (totalProcessedAmount != cart.GetTotal(_orderGroupCalculator).Amount)
            {
                throw new InvalidOperationException("Wrong amount");
            }

            if (payment.HasFraudStatus(FraudStatus.PENDING))
            {
                payment.Status = PaymentStatus.Pending.ToString();
            }

            _cartService.RequestInventory(cart);

            var orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
            var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
            _orderRepository.Delete(cart.OrderLink);

            if (purchaseOrder == null)
            {
                _klarnaCheckoutService.CancelOrder(cart);

                return null;
            }
            else
            {
                _klarnaCheckoutService.Complete(purchaseOrder);
                purchaseOrder.Properties[Klarna.Common.Constants.KlarnaOrderIdField] = klarnaOrderId;

                _orderRepository.Save(purchaseOrder);
                return purchaseOrder;
            }
        }

        public virtual bool ValidateOrder(ModelStateDictionary modelState, CheckoutViewModel viewModel, IDictionary<ILineItem, IList<ValidationIssue>> validationMessages)
        {
            PurchaseValidation validation;
            if (viewModel.IsAuthenticated)
            {
                validation = AuthenticatedPurchaseValidation;
            }
            else
            {
                validation = AnonymousPurchaseValidation;
            }

            return validation.ValidateModel(modelState, viewModel) && validation.ValidateOrderOperation(modelState, validationMessages);
        }

        public void ProcessPaymentCancel(CheckoutViewModel viewModel, TempDataDictionary tempData, ControllerContext controlerContext)
        {
            var message = tempData["message"]?.ToString() ?? controlerContext.HttpContext.Request.QueryString["message"];
            if (!string.IsNullOrEmpty(message))
            {
                viewModel.Message = message;
            }
        }
    }
}