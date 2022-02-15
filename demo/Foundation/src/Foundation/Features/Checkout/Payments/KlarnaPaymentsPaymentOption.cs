using System.Collections.Generic;
using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using Foundation.Features.Checkout.Services;
using Foundation.Infrastructure.Commerce.Markets;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Klarna.Payments;
using Klarna.Payments.Extensions;
using Klarna.Payments.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Foundation.Features.Checkout.Payments
{
    [ServiceConfiguration(typeof(IPaymentMethod), Lifecycle = ServiceInstanceScope.Transient)]
    public class KlarnaPaymentsPaymentOption : PaymentOptionBase
    {
        private readonly IKlarnaPaymentsService _klarnaPaymentsService;
        private readonly ICartService _cartService;

        public KlarnaPaymentsPaymentOption(LocalizationService localizationService, IOrderGroupFactory orderGroupFactory, ICurrentMarket currentMarket, LanguageService languageService, IPaymentService paymentService, IKlarnaPaymentsService klarnaPaymentsService, ICartService cartService) : base(localizationService, orderGroupFactory, currentMarket, languageService, paymentService)
        {
            _klarnaPaymentsService = klarnaPaymentsService;
            _cartService = cartService;
        }

        public string ClientToken { get; set; }

        public IEnumerable<PaymentMethodCategory> PaymentMethodCategories { get; private set; }

        public Descriptor Descriptor { get; private set; }


        public void Initialize()
        {
            var cart = _cartService.LoadCart(_cartService.DefaultCartName, false);
            var siteUrl = SiteUrlHelper.GetCurrentSiteUrl();
            if (AsyncHelper.RunSync(() => _klarnaPaymentsService.CreateOrUpdateSession(cart.Cart, new SessionSettings(siteUrl))))
            {
                ClientToken = cart.Cart.GetKlarnaClientToken();
                PaymentMethodCategories = cart.Cart.GetKlarnaPaymentMethodCategories();
                Descriptor = cart.Cart.GetKlarnaPaymentsDescriptor() ?? new Descriptor {Tagline = "Klarna Payments"};
            }
        }

        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = orderGroup.CreatePayment(OrderGroupFactory);
            payment.PaymentType = PaymentType.Other;
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = Constants.KlarnaPaymentSystemKeyword;
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

        public override bool ValidateData() => true;

        public override string SystemKeyword => Constants.KlarnaPaymentSystemKeyword;
    }
}