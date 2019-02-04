using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Orders;

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

        public decimal PriceIncludingTax(decimal basePrice, IEnumerable<ITaxValue> taxes, TaxType taxtype)
        {
            return basePrice + GetTaxes(basePrice, taxes, taxtype);
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

        public decimal GetTaxes(decimal basePrice, IEnumerable<ITaxValue> taxes, TaxType taxtype)
        {
            return taxes
                .Where(x => x.TaxType == taxtype)
                .Sum(x => basePrice * (decimal)x.Percentage * 0.01m);
        }

        public bool TryGetTaxCategoryId(ILineItem item, out int taxCategoryId)
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

        public bool TryGetTaxValues(IMarket market, IShipment shipment, int taxCategoryId, out ITaxValue[] taxValues)
        {
            if (shipment == null)
            {
                taxValues = Enumerable.Empty<ITaxValue>().ToArray();
                return false;
            }

            var categoryNameById = CatalogTaxManager.GetTaxCategoryNameById(taxCategoryId);

            var shipmentAddress = shipment.ShippingAddress ?? new OrderAddress { CountryCode = market.Countries.FirstOrDefault() };

            taxValues = OrderContext.Current.GetTaxes(Guid.Empty, categoryNameById, market.DefaultLanguage.Name, shipmentAddress).ToArray();
            return taxValues.Any();
        }
    }
}