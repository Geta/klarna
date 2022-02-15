using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using Foundation.Features.Checkout.Services;
using Foundation.Infrastructure.Commerce.Markets;
using Klarna.Checkout;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Foundation.Features.Checkout.Payments
{
    [ServiceConfiguration(typeof(IPaymentMethod), Lifecycle = ServiceInstanceScope.Transient)]
    public class KlarnaCheckoutPaymentOption : PaymentOptionBase
    {
        private readonly IKlarnaCheckoutService _klarnaCheckoutService;
        private readonly ICartService _cartService;

        public KlarnaCheckoutPaymentOption(LocalizationService localizationService, IOrderGroupFactory orderGroupFactory, ICurrentMarket currentMarket, LanguageService languageService, IPaymentService paymentService, IKlarnaCheckoutService klarnaCheckoutService, ICartService cartService) : base(localizationService, orderGroupFactory, currentMarket, languageService, paymentService)
        {
            _klarnaCheckoutService = klarnaCheckoutService;
            _cartService = cartService;
        }

        public string HtmlSnippet { get; set; }

        public void Initialize()
        {
            var cart = _cartService.LoadCart(_cartService.DefaultCartName, false);
            var orderData = AsyncHelper.RunSync(() => _klarnaCheckoutService.CreateOrUpdateOrder(cart.Cart));

            HtmlSnippet = orderData.HtmlSnippet;
        }

        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = orderGroup.CreatePayment(OrderGroupFactory);
            payment.PaymentType = PaymentType.Other;
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = Constants.KlarnaCheckoutSystemKeyword;
            payment.Amount = amount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = TransactionType.Authorization.ToString();
            payment.ProviderTransactionID = orderGroup.Properties[Klarna.Common.Constants.KlarnaOrderIdField]?.ToString(); 
            return payment;
        }

        public void PostProcess(IPayment payment)
        {
            if (payment.Properties[Klarna.Common.Constants.FraudStatusPaymentField]?.ToString() == FraudStatus.PENDING.ToString())
            {
                payment.Status = PaymentStatus.Pending.ToString();
            }
        }

        public override bool ValidateData() => true;

        public override string SystemKeyword => Constants.KlarnaCheckoutSystemKeyword;
    }
}