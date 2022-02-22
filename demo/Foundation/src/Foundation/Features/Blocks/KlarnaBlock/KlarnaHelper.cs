﻿using System;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Foundation.Features.CatalogContent.Services;
using Foundation.Infrastructure.Commerce.Markets;
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

        private static readonly Lazy<ICurrencyService> CurrencyService =
            new Lazy<ICurrencyService>(() => ServiceLocator.Current.GetInstance<ICurrencyService>());

        private static readonly Lazy<IPromotionService> PromotionService =
            new Lazy<IPromotionService>(() => ServiceLocator.Current.GetInstance<IPromotionService>());

        public static string GetLocale()
        {
            var culture = ContentLanguage.PreferredCulture;

            if (culture.Name.Equals("en", StringComparison.InvariantCultureIgnoreCase))
            {
                return "en-US";
            }

            return culture.Name;
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
    }
}