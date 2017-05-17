using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Klarna.Rest.Models;
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
#pragma warning restore 649

        private static string GetVariantImage(ContentReference contentReference)
        {
            VariationContent variant;
            if (_contentRepository.Service.TryGet(contentReference, out variant))
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
                0, 0, 0);
        }

        public static OrderLine GetOrderLineWithTax(this ILineItem lineItem, IMarket market, IShipment shipment, Currency currency, bool includeProductAndImageUrl = false)
        {
            (int unitPrice, int taxRate, int totalDiscountAmount, int totalAmount, int totalTaxAmount) = GetPrices(lineItem, market, shipment, currency);
            return GetOrderLine(
                lineItem, 
                includeProductAndImageUrl, 
                unitPrice, 
                totalAmount,
                totalDiscountAmount,
                totalTaxAmount, 
                taxRate);
        }

        private static OrderLine GetOrderLine(ILineItem lineItem, bool includeProductAndImageUrl, int unitPrice, int totalAmount, int totalDiscountAmount, int totalTaxAmount, int taxRate)
        {
            var orderLine = new PatchedOrderLine
            {
                Quantity = (int)lineItem.Quantity,
                Name = lineItem.DisplayName,
                Reference = lineItem.Code,
                Type = "physical"
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
                    orderLine.ProductUrl = _urlResolver.Service.GetUrl(contentLink);
                    orderLine.ProductImageUrl = GetVariantImage(contentLink);
                }
            }
            return orderLine;
        }

        public static (int unitPrice, int taxRate, int totalDiscountAmount, int totalAmount, int totalTaxAmount) GetPrices(ILineItem lineItem, IMarket market, IShipment shipment, Currency currency)
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
            var extendedPrice = lineItem.GetDiscountedPrice(currency).Amount;
            var discountAmount = (totalPriceWithoutDiscount - extendedPrice);

            // Tax value
            var taxValues = _lineItemTaxCalculator.Service.GetTaxValuesForLineItem(lineItem, market, shipment);
            var taxPercentage = taxValues
                .Where(x => x.TaxType == taxType)
                .Sum(x => (decimal)x.Percentage);

            // Includes tax, excludes discount. (max value: 100000000)
            var unitPriceIncludingTax = AmountHelper.GetAmount(_lineItemTaxCalculator.Service.PriceIncludingTax(unitPrice, taxValues, taxType));
            // Non - negative minor units. Includes tax
            int totalDiscountAmount = AmountHelper.GetAmount(_lineItemTaxCalculator.Service.PriceIncludingTax(discountAmount, taxValues, taxType));
            // Includes tax and discount. Must match (quantity * unit_price) - total_discount_amount within quantity. (max value: 100000000)
            var totalAmount = AmountHelper.GetAmount(_lineItemTaxCalculator.Service.PriceIncludingTax(extendedPrice, taxValues, taxType));
            // Non-negative. In percent, two implicit decimals. I.e 2500 = 25%.
            var taxRate = AmountHelper.GetAmount(taxPercentage);
            // Must be within 1 of total_amount - total_amount * 10000 / (10000 + tax_rate). Negative when type is discount.
            var totalTaxAmount = AmountHelper.GetAmount(_lineItemTaxCalculator.Service.GetTaxes(extendedPrice, taxValues, taxType));

            return (unitPriceIncludingTax, taxRate, totalDiscountAmount, totalAmount, totalTaxAmount);
        }
    }
}