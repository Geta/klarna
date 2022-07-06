using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Foundation.Features.CatalogContent.Services;
using Foundation.Features.Checkout.Services;
using Foundation.Features.MyAccount.AddressBook;
using Foundation.Infrastructure.Commerce.Markets;
using Klarna.Common;
using Klarna.Common.Helpers;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using PriceCalculationService = Foundation.Features.Checkout.PriceCalculationService;

namespace Foundation.Features.Blocks.KlarnaBlock
{
    public static class KlarnaHelper
    {
        private static readonly Lazy<ICurrentMarket> CurrentMarket =
            new Lazy<ICurrentMarket>(() => ServiceLocator.Current.GetInstance<ICurrentMarket>());

        private static readonly Lazy<IAddressBookService> AddressBookService =
            new Lazy<IAddressBookService>(() => ServiceLocator.Current.GetInstance<IAddressBookService>());

        private static readonly Lazy<ILanguageService> LanguageService =
            new Lazy<ILanguageService>(() => ServiceLocator.Current.GetInstance<ILanguageService>());

        private static readonly Lazy<ICurrencyService> CurrencyService =
            new Lazy<ICurrencyService>(() => ServiceLocator.Current.GetInstance<ICurrencyService>());

        private static readonly Lazy<IPromotionService> PromotionService =
            new Lazy<IPromotionService>(() => ServiceLocator.Current.GetInstance<IPromotionService>());

        private static readonly Lazy<ICartService> CartService =
            new Lazy<ICartService>(() => ServiceLocator.Current.GetInstance<ICartService>());

        private static readonly Lazy<IOrderGroupCalculator> OrderGroupCalculator =
            new Lazy<IOrderGroupCalculator>(() => ServiceLocator.Current.GetInstance<IOrderGroupCalculator>());

        public static string GetLocale()
        {
            // The language and the billing country where country code is the ISO 3166-1 alpha-2 code. Example: en-SE (English and Swedish market)
            var culture = LanguageService.Value.GetPreferredCulture();
            var locale = LanguageService.Value.ConvertToLocale(culture.Name);
            var market = CurrentMarket.Value.GetCurrentMarket();
            var countryCode = "US"; // our default

            // If logged in we get the country for the customer from their preferred address.
            var billingAddress = AddressBookService.Value.GetPreferredBillingAddress();

            if (billingAddress != null && !string.IsNullOrEmpty(billingAddress.CountryCode))
            {
                countryCode = CountryCodeHelper.GetTwoLetterCountryCode(billingAddress.CountryCode);
            }
            else
            {
                if (market.Countries.Any())
                {
                    // We use the first country of the market as billing country
                    countryCode = CountryCodeHelper.GetTwoLetterCountryCode(market.Countries.FirstOrDefault());
                }
            }

            return $"{locale}-{countryCode}"; // Example: en-US, en-SE, sv-SE
        }

        public static int GetPurchasePrice(string code)
        {
            var market = CurrentMarket.Value.GetCurrentMarket();
            var currency = CurrencyService.Value.GetCurrentCurrency();

            var price = PriceCalculationService.GetSalePrice(code, market.MarketId, currency);
            if (price == null)
            {
                return 0;
            }

            var discountPrice = price;
            if (price.UnitPrice.Amount > 0 && !string.IsNullOrEmpty(code))
            {
                discountPrice = PromotionService.Value.GetDiscountPrice(new CatalogKey(code), market.MarketId, currency);
            }

            return AmountHelper.GetAmount(discountPrice.UnitPrice.Amount);
        }

        public static int GetCartTotal()
        {
            var cart = CartService.Value.LoadCart(CartService.Value.DefaultCartName, false);

            if (cart?.Cart == null)
            {
                return 0;
            }

            var totals = OrderGroupCalculator.Value.GetOrderGroupTotals(cart.Cart);

            return AmountHelper.GetAmount(totals.Total.Amount);
        }
    }
}