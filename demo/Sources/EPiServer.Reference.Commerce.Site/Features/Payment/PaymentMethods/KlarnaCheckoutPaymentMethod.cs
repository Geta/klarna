using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders;
using System.ComponentModel;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using Klarna.Checkout;
using Klarna.Common;
using Klarna.Payments.Models;
using Constants = Klarna.Checkout.Constants;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    [ServiceConfiguration(typeof(IPaymentMethod))]
    public class KlarnaCheckoutPaymentMethod : PaymentMethodBase, IDataErrorInfo
    {
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly ICartService _cartService;
        private readonly IContentLoader _contentLoader;
        private readonly IKlarnaCheckoutService _klarnaCheckoutService;

        public override string SystemKeyword => Constants.KlarnaCheckoutSystemKeyword;

        public KlarnaCheckoutPaymentMethod(
            LocalizationService localizationService,
            IOrderGroupFactory orderGroupFactory,
            LanguageService languageService,
            IPaymentManagerFacade paymentManager,
            ICartService cartService,
            IContentLoader contentLoader,
            IKlarnaCheckoutService klarnaCheckoutService)
            : base(localizationService, orderGroupFactory, languageService, paymentManager)
        {
            _orderGroupFactory = orderGroupFactory;
            _cartService = cartService;
            _contentLoader = contentLoader;
            _klarnaCheckoutService = klarnaCheckoutService;

            InitializeValues();
        }

        public void InitializeValues()
        {
            var startPage = _contentLoader.GetStartPage();

            if (!startPage.KlarnaCheckoutEnabled && !IsActive)
            {
                return;
            }
            var cart = _cartService.LoadCart(_cartService.DefaultCartName);
            var orderData = AsyncHelper.RunSync(() => _klarnaCheckoutService.CreateOrUpdateOrder(cart));

            HtmlSnippet = orderData.HtmlSnippet;
        }

        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = orderGroup.CreatePayment(_orderGroupFactory);
            payment.PaymentType = PaymentType.Other;
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = Constants.KlarnaCheckoutSystemKeyword;
            payment.Amount = amount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = TransactionType.Authorization.ToString();
            return payment;
        }

        public void PostProcess(IPayment payment)
        {
            if (payment.Properties[Klarna.Common.Constants.FraudStatusPaymentField]?.ToString() == FraudStatus.PENDING.ToString())
            {
                payment.Status = PaymentStatus.Pending.ToString();
            }
        }

        public override bool ValidateData()
        {
            return true;
        }

        public string this[string columnName] => string.Empty;

        public string Error { get; }

        public string HtmlSnippet { get; set; }
    }
}