using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Globalization;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Klarna.Checkout;
using Klarna.Payments.Helpers;
using Klarna.Rest.Models;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Exceptions;
using Mediachase.Commerce.Orders.Managers;

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
        private readonly IPromotionEngine _promotionEngine;
        private readonly ILogger _log = LogManager.GetLogger(typeof(CheckoutService));
        private readonly IContentLoader _contentLoader; 
        private readonly IKlarnaCheckoutService _klarnaCheckoutService;
        private readonly CheckoutViewModelFactory _checkoutViewModelFactory;

      

        public AuthenticatedPurchaseValidation AuthenticatedPurchaseValidation { get; private set; }
        public AnonymousPurchaseValidation AnonymousPurchaseValidation { get; private set; }
        public CheckoutAddressHandling CheckoutAddressHandling { get; private set; }

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
            IPromotionEngine promotionEngine, 
            IContentLoader contentLoader, 
            IKlarnaCheckoutService klarnaCheckoutService, 
            CheckoutViewModelFactory checkoutViewModelFactory)
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
            _promotionEngine = promotionEngine;
            _contentLoader = contentLoader;
            _klarnaCheckoutService = klarnaCheckoutService;
            _checkoutViewModelFactory = checkoutViewModelFactory;

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

        public virtual void ApplyDiscounts(ICart cart)
        {
            cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings());
        }

        public virtual void CreateAndAddPaymentToCart(ICart cart, CheckoutViewModel viewModel)
        {
            var total = cart.GetTotal(_orderGroupCalculator);
            var payment = viewModel.Payment.PaymentMethod.CreatePayment(total.Amount, cart);
            if (payment.PaymentMethodName.Equals(Klarna.Payments.Constants.KlarnaPaymentSystemKeyword))
            {
                payment.Properties[Klarna.Payments.Constants.AuthorizationTokenPaymentMethodField] = viewModel.AuthorizationToken;
            }

            cart.AddPayment(payment, _orderGroupFactory);
            payment.BillingAddress = _addressBookService.ConvertToAddress(viewModel.BillingAddress, cart);
        }

        public virtual IPurchaseOrder PlaceOrder(ICart cart, ModelStateDictionary modelState, CheckoutViewModel checkoutViewModel)
        {
            try
            {
                cart.ProcessPayments(_paymentProcessor, _orderGroupCalculator);

                var totalProcessedAmount = cart.GetFirstForm().Payments.Where(x => x.Status.Equals(PaymentStatus.Processed.ToString())).Sum(x => x.Amount);
                if (totalProcessedAmount != cart.GetTotal(_orderGroupCalculator).Amount)
                {
                    throw new InvalidOperationException("Wrong amount");
                }

                var payment = cart.GetFirstForm().Payments.First();
                checkoutViewModel.Payment.PaymentMethod.PostProcess(payment);
                
                var orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
                var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
                _orderRepository.Delete(cart.OrderLink);

                return purchaseOrder;
            }
            catch (PaymentException ex)
            {
                modelState.AddModelError("", ex.Message + _localizationService.GetString("/Checkout/Payment/Errors/ProcessingPaymentFailure"));
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

            var startpage = _contentRepository.Get<StartPage>(ContentReference.StartPage);
            var confirmationPage = _contentRepository.GetFirstChild<OrderConfirmationPage>(viewModel.CurrentPage.ContentLink);

            try
            {
                _mailService.Send(startpage.OrderConfirmationMail, queryCollection, viewModel.BillingAddress.Email, confirmationPage.Language.Name);
            }
            catch (Exception e)
            {
                _log.Warning(string.Format("Unable to send purchase receipt to '{0}'.", viewModel.BillingAddress.Email), e);
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

            var confirmationPage = _contentRepository.GetFirstChild<OrderConfirmationPage>(checkoutViewModel.CurrentPage.ContentLink);

            return new UrlBuilder("http://klarna.localtest.me/" + confirmationPage.LinkURL) {QueryCollection = queryCollection}.ToString();
        }

        public IPurchaseOrder CreateKlarnaOrder(
            string klarnaOrderId, 
            CheckoutOrderData order, 
            ICart cart,
            ModelStateDictionary modelState,
            out CheckoutViewModel viewModel)
        {
            var contentLink = _contentLoader.Get<StartPage>(ContentReference.StartPage).CheckoutPage;

            viewModel = _checkoutViewModelFactory.CreateCheckoutViewModel(cart, _contentLoader.Get<CheckoutPage>(contentLink));

            var paymentRow =
                PaymentManager.GetPaymentMethodBySystemName(Klarna.Checkout.Constants.KlarnaCheckoutSystemKeyword,
                        ContentLanguage.PreferredCulture.Name)
                    .PaymentMethod.FirstOrDefault();
            var paymentViewModel = new PaymentMethodViewModel<KlarnaCheckoutPaymentMethod>
            {
                PaymentMethodId = paymentRow.PaymentMethodId,
                SystemName = paymentRow.SystemKeyword,
                FriendlyName = paymentRow.Name,
                Description = paymentRow.Description,
                PaymentMethod = new KlarnaCheckoutPaymentMethod()
            };

            viewModel.Payment = paymentViewModel;
            viewModel.Payment.PaymentMethod.PaymentMethodId = paymentRow.PaymentMethodId;

            viewModel.BillingAddress = new AddressModel
            {
                Name =
                    $"{order.BillingAddress.StreetAddress}{order.BillingAddress.StreetAddress2}{order.BillingAddress.City}",
                FirstName = order.BillingAddress.GivenName,
                LastName = order.BillingAddress.FamilyName,
                Email = order.BillingAddress.Email,
                DaytimePhoneNumber = order.BillingAddress.Phone,
                Line1 = order.BillingAddress.StreetAddress,
                Line2 = order.BillingAddress.StreetAddress2,
                PostalCode = order.BillingAddress.PostalCode,
                City = order.BillingAddress.City,
                CountryName = order.BillingAddress.Country
            };

            CreateAndAddPaymentToCart(cart, viewModel);

            var purchaseOrder = PlaceOrder(cart, modelState, viewModel);
            if (purchaseOrder == null) //something went wrong while creating a purchase order, cancel  order at Klarna
            {
                _klarnaCheckoutService.CancelOrder(cart);
                return null;
            }

            purchaseOrder.Properties[Klarna.Common.Constants.KlarnaOrderIdField] = klarnaOrderId;

            _orderRepository.Save(purchaseOrder);
            return purchaseOrder;
        }
    }
}