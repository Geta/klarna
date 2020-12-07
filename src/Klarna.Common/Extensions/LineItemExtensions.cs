using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;

namespace Klarna.Common.Extensions
{
    public static class LineItemExtensions
    {
#pragma warning disable 649
        private static Injected<ReferenceConverter> _referenceConverter;
        private static Injected<UrlResolver> _urlResolver;
        private static Injected<IContentRepository> _contentRepository;
        private static Injected<ILineItemTaxCalculator> _lineItemTaxCalculator;
        private static readonly int MaxOrderLineReference = 64;
        private static Injected<ITaxCalculator> _taxCalculator;
        private static Injected<ILanguageService> _languageService;
#pragma warning restore 649

        private static string GetVariantImage(ContentReference contentReference)
        {
            if (_contentRepository.Service.TryGet(contentReference, out VariationContent variant))
            {
                return variant.CommerceMediaCollection.Select(media => _urlResolver.Service.GetUrl(media.AssetLink))
                    .FirstOrDefault();
            }
            return string.Empty;
        }

        public static OrderLine GetOrderLine(this ILineItem lineItem, bool includeProductAndImageUrl = false)
        {
            return GetOrderLine(
                lineItem,
                includeProductAndImageUrl,
                AmountHelper.GetAmount(lineItem.PlacedPrice),
                AmountHelper.GetAmount(lineItem.PlacedPrice * lineItem.Quantity),
                AmountHelper.GetAmount(lineItem.GetEntryDiscount()), 0, 0);
        }

        public static OrderLine GetOrderLineWithTax(
            this ILineItem lineItem,
            IMarket market,
            IShipment shipment,
            Currency currency,
            bool includeProductAndImageUrl = false)
        {
            var prices = GetPrices(lineItem, market, shipment, currency);
            return GetOrderLine(
                lineItem,
                includeProductAndImageUrl,
                prices.UnitPrice,
                prices.TotalAmount,
                prices.TotalDiscountAmount,
                prices.TotalTaxAmount,
                prices.TaxRate);
        }

        private static OrderLine GetOrderLine(
            ILineItem lineItem,
            bool includeProductAndImageUrl,
            int unitPrice,
            int totalAmount,
            int totalDiscountAmount,
            int totalTaxAmount,
            int taxRate)
        {
            var orderLine = new OrderLine
            {
                Quantity = (int)lineItem.Quantity,
                Name = lineItem.DisplayName,
                Reference = lineItem.Code.Length > 64
                    ? lineItem.Code.Substring(0, (MaxOrderLineReference - 1))
                    : lineItem.Code, // can't use more then 64 characters for the order reference
                Type = OrderLineType.physical
            };

            if (string.IsNullOrEmpty(orderLine.Name))
            {
                var entry = lineItem.GetEntryContent();
                if (entry != null)
                {
                    orderLine.Name = entry.DisplayName;
                }
            }

            orderLine.UnitPrice = unitPrice;
            orderLine.TotalAmount = totalAmount;
            orderLine.TotalDiscountAmount = totalDiscountAmount;
            orderLine.TotalTaxAmount = totalTaxAmount;
            orderLine.TaxRate = taxRate;

            if (includeProductAndImageUrl)
            {
                var contentLink = _referenceConverter.Service.GetContentLink(lineItem.Code);
                if (!ContentReference.IsNullOrEmpty(contentLink))
                {
                    orderLine.ProductUrl = _urlResolver.Service.GetUrl(contentLink, _languageService.Service.GetPreferredCulture().Name, new VirtualPathArguments { ValidateTemplate = false }).ToAbsoluteUrl();
                    orderLine.ImageUrl = GetVariantImage(contentLink).ToAbsoluteUrl();
                }
            }
            return orderLine;
        }

        public static Prices GetPrices(ILineItem lineItem, IMarket market, IShipment shipment, Currency currency)
        {
            /*
            unit_price integer required
                Minor units. Includes tax, excludes discount. (max value: 100000000)
            tax_rate integer required
                Non-negative. In percent, two implicit decimals. I.e 2500 = 25%.
            total_amount integer required
                Includes tax and discount. Must match (quantity * unit_price) - total_discount_amount within �quantity. (max value: 100000000)
            total_tax_amount integer required
                Must be within �1 of total_amount - total_amount * 10000 / (10000 + tax_rate). Negative when type is discount.
            total_discount_amount integer
                Non - negative minor units. Includes tax.
            */
            var taxType = TaxType.SalesTax;

            // All excluding tax
            var unitPrice = lineItem.PlacedPrice;
            var totalPriceWithoutDiscount = lineItem.PlacedPrice * lineItem.Quantity;
            var extendedPrice = lineItem.GetDiscountedPrice(currency);
            var discountAmount = (totalPriceWithoutDiscount - extendedPrice);

            // Tax value
            var taxValues = _lineItemTaxCalculator.Service.GetTaxValuesForLineItem(lineItem, market, shipment);
            var taxPercentage = taxValues
                .Where(x => x.TaxType == taxType)
                .Sum(x => (decimal)x.Percentage);

            // Using ITaxCalculator instead of ILineItemCalculator because ILineItemCalculator
            // calculates tax from the price which includes order discount amount and line item discount amount
            // but should use only line item discount amount
            var salesTax =
                _taxCalculator.Service.GetSalesTax(lineItem, market, shipment.ShippingAddress, extendedPrice);

            // Includes tax, excludes discount. (max value: 100000000)
            var unitPriceIncludingTax = AmountHelper.GetAmount(
                _lineItemTaxCalculator.Service.PriceIncludingTaxPercent(unitPrice, taxPercentage, market));
            // Non - negative minor units. Includes tax
            var totalDiscountAmount = AmountHelper.GetAmount(
                _lineItemTaxCalculator.Service.PriceIncludingTaxPercent(discountAmount, taxPercentage, market));
            // Includes tax and discount. Must match (quantity * unit_price) - total_discount_amount within quantity. (max value: 100000000)
            var totalAmount = AmountHelper.GetAmount(
                _lineItemTaxCalculator.Service.PriceIncludingTaxAmount(extendedPrice, salesTax.Amount, market));
            // Non-negative. In percent, two implicit decimals. I.e 2500 = 25%.
            var taxRate = AmountHelper.GetAmount(taxPercentage);
            // Must be within 1 of total_amount - total_amount * 10000 / (10000 + tax_rate). Negative when type is discount.
            var totalTaxAmount = AmountHelper.GetAmount(salesTax.Amount);

            return new Prices(unitPriceIncludingTax, taxRate, totalDiscountAmount, totalAmount, totalTaxAmount);
        }
    }
}