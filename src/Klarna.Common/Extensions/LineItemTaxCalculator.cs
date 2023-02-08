using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce;
using System.Collections.Generic;
using System.Linq;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders.Managers;
using System.Data;
using EPiServer.ServiceLocation;

namespace Klarna.Common.Extensions
{
    [ServiceConfiguration(typeof(ILineItemTaxCalculator))]
    public class LineItemTaxCalculator : ILineItemTaxCalculator
    {
        private ReferenceConverter _referenceConverter;
        private IContentRepository _contentRepository;

        public LineItemTaxCalculator(
            ReferenceConverter referenceConverter,
            IContentRepository contentRepository)
        {
            _referenceConverter = referenceConverter;
            _contentRepository = contentRepository;
        }

        public decimal PriceIncludingTaxAmount(decimal basePrice, decimal taxAmount, IMarket market)
        {
            if (market.PricesIncludeTax) return basePrice;
            return basePrice + taxAmount;
        }

        public decimal PriceIncludingTaxPercent(decimal basePrice, decimal taxPercent, IMarket market)
        {
            if (market.PricesIncludeTax) return basePrice;
            return basePrice * taxPercent * 0.01m + basePrice;
        }

        public ITaxValue[] GetTaxValuesForLineItem(ILineItem lineItem, IMarket market, IShipment shipment)
        {
            if (TryGetTaxCategoryId(lineItem, out int taxCategoryId) &&
                TryGetTaxValues(market, shipment, taxCategoryId, out ITaxValue[] taxValues))
            {
                return taxValues;
            }
            return Enumerable.Empty<ITaxValue>().ToArray();
        }

        private bool TryGetTaxCategoryId(ILineItem item, out int taxCategoryId)
        {
            var contentLink = _referenceConverter.GetContentLink(item.Code);
            if (ContentReference.IsNullOrEmpty(contentLink))
            {
                taxCategoryId = 0;
                return false;
            }
            var pricing = _contentRepository.Get<EntryContentBase>(contentLink) as IPricing;
            if (pricing?.TaxCategoryId == null)
            {
                taxCategoryId = 0;
                return false;
            }

            taxCategoryId = pricing.TaxCategoryId.Value;
            return true;
        }

        private bool TryGetTaxValues(IMarket market, IShipment shipment, int taxCategoryId, out ITaxValue[] taxValues)
        {
            if (shipment == null)
            {
                taxValues = Enumerable.Empty<TaxValue>().ToArray();
                return false;
            }

            var temp = new List<TaxValue>();

            var taxes = TaxManager.GetTaxes(CatalogTaxManager.GetTaxCategoryNameById(taxCategoryId), market.DefaultLanguage.Name, shipment.ShippingAddress.CountryCode, shipment.ShippingAddress.RegionName, shipment.ShippingAddress.PostalCode, string.Empty, string.Empty, shipment.ShippingAddress.City);
            if (taxes.Rows.Count > 0)
            {
                foreach (DataRow row in taxes.Rows)
                {
                    temp.Add(new TaxValue((double)row["Percentage"], (string)row["Name"], (string)row["Name"], (TaxType)row["TaxType"]));
                }

                taxValues = temp.ToArray();

                return taxValues.Any();
            }

            taxValues = null;
            return false;
        }
    }
}
