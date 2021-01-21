using System.Collections.Generic;
using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders;
using System.ComponentModel;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Web;
using Klarna.Common;
using Klarna.Payments;
using Klarna.Payments.Extensions;
using Klarna.Payments.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Managers;
using Constants = Klarna.Payments.Constants;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    [ServiceConfiguration(typeof(IPaymentMethod), Lifecycle = ServiceInstanceScope.Hybrid)]
    public class KlarnaPaymentsPaymentMethod : PaymentMethodBase, IDataErrorInfo
    {
        private string _klarnaLogoUrl;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly ICartService _cartService;
        private readonly IContentLoader _contentLoader;
        private readonly ICurrentMarket _currentMarket;
        private readonly IKlarnaPaymentsService _klarnaPaymentsService;

        public override string SystemKeyword => Constants.KlarnaPaymentSystemKeyword;

        public KlarnaPaymentsPaymentMethod()
            : this(
                LocalizationService.Current,
                ServiceLocator.Current.GetInstance<IOrderGroupFactory>(),
                ServiceLocator.Current.GetInstance<LanguageService>(),
                ServiceLocator.Current.GetInstance<IPaymentManagerFacade>(),
                ServiceLocator.Current.GetInstance<ICartService>(),
                ServiceLocator.Current.GetInstance<IContentLoader>(),
                ServiceLocator.Current.GetInstance<ICurrentMarket>(),
                ServiceLocator.Current.GetInstance<IKlarnaPaymentsService>())
        {
        }

        public KlarnaPaymentsPaymentMethod(
                LocalizationService localizationService,
                IOrderGroupFactory orderGroupFactory,
                LanguageService languageService,
                IPaymentManagerFacade paymentManager,
                ICartService cartService,
                IContentLoader contentLoader,
                ICurrentMarket currentMarket,
                IKlarnaPaymentsService klarnaPaymentsService)
            : base(localizationService, orderGroupFactory, languageService, paymentManager)
        {
            _orderGroupFactory = orderGroupFactory;
            _cartService = cartService;
            _contentLoader = contentLoader;
            _currentMarket = currentMarket;
            _klarnaPaymentsService = klarnaPaymentsService;

            InitializeValues();
        }

        public void InitializeValues()
        {
            var startPage = _contentLoader.GetStartPage();

            if (!startPage.KlarnaPaymentsEnabled && !IsActive)
            {
                return;
            }

            var cart = _cartService.LoadCart(_cartService.DefaultCartName);
            var siteUrl = SiteDefinition.Current.SiteUrl;
            if (AsyncHelper.RunSync(() => _klarnaPaymentsService.CreateOrUpdateSession(cart, new SessionSettings(siteUrl))))
            {
                ClientToken = cart.GetKlarnaClientToken();
                PaymentMethodCategories = cart.GetKlarnaPaymentMethodCategories();
            }
        }


        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = orderGroup.CreatePayment(_orderGroupFactory);
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

        public override bool ValidateData()
        {
            return true;
        }

        public string this[string columnName] => string.Empty;

        public string Error { get; }

        public string ClientToken { get; set; }

        public IEnumerable<PaymentMethodCategory> PaymentMethodCategories { get; private set; }

        public string KlarnaLogoUrl
        {
            get
            {
                if (_klarnaLogoUrl != null)
                {
                    return _klarnaLogoUrl;
                }

                var paymentMethodDto = PaymentManager.GetPaymentMethod(PaymentMethodId);
                var config = paymentMethodDto.GetKlarnaPaymentsConfiguration(_currentMarket.GetCurrentMarket().MarketId);
                _klarnaLogoUrl = config.LogoUrl;

                return _klarnaLogoUrl;
            }
        }
    }
}