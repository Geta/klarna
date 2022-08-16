# Klarna On-site messsaging

On-site messaging is a platform that enables you to add tailored messaging to your website. With On-site messaging you can let shoppers know about the different payment options you have available as they browse your site. By using Klarna, customers have access to our flexible payment options in the checkout; On-site messaging is a great way to let them know even before they decide to buy.

There are assets designed for all of the relevant pages of your website. All of these assets are available in the Merchant Portal and are free to use. Every asset is dynamic, adjusting to the payment methods that you offer.

[More information](https://docs.klarna.com/on-site-messaging/overview/).

## Installation

Before installing On-site messaging you need to [activate it in the Merchant Portal](https://docs.klarna.com/on-site-messaging/get-started/activation/). 

Once activated you can add the JavaScript Library needed to run it. The JavaScript Library snippet is generated for you and can be obtained in the installation page in the On-site messaging panel in the Merchant Portal. Log in to the Merchant Portal and go to the On-site messaging application. Once there, you will be redirected to the Installation section of the application.

## Sample code

We've included a sample block below that uses the default placement options. Note that this requires the JavaScript library to work (obtained under installation).

KlarnaBlock.cs
```csharp
using System.ComponentModel.DataAnnotations;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using Foundation.Features.Shared;
using Foundation.Infrastructure;

namespace Foundation.Features.Blocks.KlarnaBlock
{
    [ContentType(DisplayName = "Klarna Block",
        GUID = "B5735027-04DE-44F0-A397-283C4DF46B9E",
        Description = "Klarna on-site messaging block",
        GroupName = GroupNames.Content)]
    [ImageUrl("/icons/cms/blocks/CMS-icon-block-03.png")]
    public class KlarnaBlock : FoundationBlockData
    {
        [SelectOne(SelectionFactoryType = typeof(KlarnaPlacementsSelectionFactory))]
        [Display(Name = "Placements", Order = 5)]
        public virtual string Placements { get; set; }
    }
}
	
public class KlarnaPlacementsSelectionFactory : ISelectionFactory
{
	public virtual IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
	{
		return new ISelectItem[]
		{
			new SelectItem { Text = "footer-promotion-auto-size", Value = "footer-promotion-auto-size" },
			new SelectItem { Text = "credit-promotion-auto-size", Value = "credit-promotion-auto-size" },
			new SelectItem { Text = "sidebar-promotion-auto-size", Value = "sidebar-promotion-auto-size" },
			new SelectItem { Text = "top-strip-promotion-auto-size", Value = "top-strip-promotion-auto-size" },
			new SelectItem { Text = "credit-promotion-badge", Value = "credit-promotion-badge" },
			new SelectItem { Text = "info-page", Value = "info-page" },
			new SelectItem { Text = "top-strip-promotion-badge", Value = "top-strip-promotion-badge" },
			new SelectItem { Text = "homepage-promotion-tall", Value = "homepage-promotion-tall" },
			new SelectItem { Text = "homepage-promotion-wide", Value = "homepage-promotion-wide" },
			new SelectItem { Text = "homepage-promotion-box", Value = "homepage-promotion-box" },
		};
	}
}
```

KlarnaBlock.cshtml
```csharp
@using Foundation.Features.Blocks.KlarnaBlock

@model IBlockViewModel<KlarnaBlock>

@Html.FullRefreshPropertiesMetaData(new[] { "Placements" })

<div style="background-color: @Model.CurrentBlock.BackgroundColor; opacity: @Model.CurrentBlock.BlockOpacity;" class="klarna-block @(Model.CurrentBlock.Padding + " " + Model.CurrentBlock.Margin)">
    <klarna-placement data-key="@Model.CurrentBlock.Placements"
                      data-locale="@KlarnaHelper.GetLocale()"
                      data-purchase-amount="@KlarnaHelper.GetCartTotal()"></klarna-placement>
</div>
```

KlarnaHelper.cs
```csharp
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
```

In Features/NamedCart/Cart.js we've added some sample code that shows how to update the purchase amount when the cart changes.
```js
changeInfoCart(result) {
    $('.largecart-Subtotal').html("$" + result.data.SubTotal.Amount);
    $('.largecart-TotalDiscount').html("$" + result.data.TotalDiscount.Amount);
    $('.largecart-TaxTotal').html("$" + result.data.TaxTotal.Amount);
    $('.largecart-ShippingTotal').html("$" + result.data.ShippingTotal.Amount);
    $('.largecart-Total').html("$" + result.data.Total.Amount);
      console.log(result.data);

      // Update purchase amount in all Klarna Placement elements
      document.querySelectorAll('klarna-placement').forEach((klarnaPlacement) => {
          klarnaPlacement.dataset.purchaseAmount = result.data.Total.Amount * 100;
      });

      window.KlarnaOnsiteService = window.KlarnaOnsiteService || [];
      window.KlarnaOnsiteService.push({ eventName: 'refresh-placements' });

      cartHelper.setCartReload(result.data.CountItems);
  }
```
