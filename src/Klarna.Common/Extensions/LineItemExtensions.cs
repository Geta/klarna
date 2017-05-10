using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Klarna.Rest.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Orders;

namespace Klarna.Common.Extensions
{
    public static class LineItemExtensions
    {
#pragma warning disable 649
        private static Injected<ReferenceConverter> _referenceConverter;
        private static Injected<UrlResolver> _urlResolver;
        private static Injected<IContentRepository> _contentRepository;
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

        public static OrderLine GetOrderLine(this ILineItem lineItem, IMarket market, IShipment shipment, Currency currency, bool includeProductAndImageUrl = false)
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

            (int unitPrice, int taxRate, int totalDiscountAmount, int totalAmount, int totalTaxAmount) = GetPrices(lineItem, market, shipment, currency);
            // Prices should include tax?
            orderLine.UnitPrice = unitPrice;
            orderLine.TotalDiscountAmount = totalDiscountAmount;
            orderLine.TotalAmount = totalAmount;
            orderLine.TotalTaxAmount = totalTaxAmount;
            // TODO Tax, check if it also works with payments
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
                Includes tax and discount. Must match (quantity * unit_price) - total_discount_amount within ±quantity. (max value: 100000000)
            total_tax_amount integer required
                Must be within ±1 of total_amount - total_amount * 10000 / (10000 + tax_rate). Negative when type is discount.
            total_discount_amount integer
                Non - negative minor units. Includes tax.
            */

            var placedPriceExcludingTax = lineItem.PlacedPrice;
            var totalDiscountExcludingTax = lineItem.GetEntryDiscount();
            (decimal taxForLineItem, decimal taxPercentage) = GetTaxForLineItem(lineItem.PlacedPrice, lineItem, market, shipment);
            var totalDiscount = totalDiscountExcludingTax * (100 + taxPercentage) / 100;

            // Includes tax, excludes discount. (max value: 100000000)
            var unitPrice = AmountHelper.GetAmount(placedPriceExcludingTax + taxForLineItem);
            // Non - negative minor units. Includes tax
            int totalDiscountAmount = AmountHelper.GetAmount(totalDiscount);
            // Includes tax and discount. Must match (quantity * unit_price) - total_discount_amount within quantity. (max value: 100000000)
            var totalAmount = (int) (lineItem.Quantity * unitPrice - totalDiscountAmount);
            // Non-negative. In percent, two implicit decimals. I.e 2500 = 25%.
            var taxRate = AmountHelper.GetAmount(taxPercentage);
            // Must be within 1 of total_amount - total_amount * 10000 / (10000 + tax_rate). Negative when type is discount.
            var totalTaxAmount = totalAmount - totalAmount * 10000 / (10000 + taxRate);

            return (unitPrice, taxRate, totalDiscountAmount, totalAmount, totalTaxAmount);
        }

        private static (decimal taxForLineItem, decimal taxPercentage) GetTaxForLineItem(decimal unitPrice, ILineItem lineItem, IMarket market, IShipment shipment)
        {
            decimal taxForLineItem = 0;
            decimal taxPercentage = 0;

            if (TryGetTaxCategoryId(lineItem, out int taxCategoryId) &&
                TryGetTaxValues(market, shipment, taxCategoryId, out ITaxValue[] taxValues))
            {
                taxForLineItem = GetTaxes(taxValues, TaxType.SalesTax, unitPrice);

                taxPercentage = taxValues
                    .Where(x => x.TaxType == TaxType.SalesTax)
                    .Average(x => (decimal)x.Percentage);

            }
            return (taxForLineItem, taxPercentage);
        }

        private static bool TryGetTaxCategoryId(ILineItem item, out int taxCategoryId)
        {
            var contentLink = _referenceConverter.Service.GetContentLink(item.Code);
            if (ContentReference.IsNullOrEmpty(contentLink))
            {
                taxCategoryId = 0;
                return false;
            }
            var pricing = _contentRepository.Service.Get<EntryContentBase>(contentLink) as IPricing;
            if (pricing?.TaxCategoryId == null)
            {
                taxCategoryId = 0;
                return false;
            }
            taxCategoryId = pricing.TaxCategoryId.Value;
            return true;
        }

        private static bool TryGetTaxValues(IMarket market, IShipment shipment, int taxCategoryId, out ITaxValue[] taxValues)
        {
            if (shipment == null)
            {
                taxValues = Enumerable.Empty<ITaxValue>().ToArray();
                return false;
            }

            var categoryNameById = CatalogTaxManager.GetTaxCategoryNameById(taxCategoryId);

            var shipmentAddress = shipment.ShippingAddress ?? new OrderAddress {CountryCode = market.Countries.FirstOrDefault()};
            
            taxValues = OrderContext.Current.GetTaxes(Guid.Empty, categoryNameById, market.DefaultLanguage.Name, shipmentAddress).ToArray();
            return taxValues.Any();
        }

        private static Decimal GetTaxes(IEnumerable<ITaxValue> taxes, TaxType taxtype, Decimal basePrice)
        {
            return taxes
                .Where(x => x.TaxType == taxtype)
                .Sum(x => basePrice * (Decimal)x.Percentage * new Decimal(1, 0, 0, false, 2));
        }
    }
}