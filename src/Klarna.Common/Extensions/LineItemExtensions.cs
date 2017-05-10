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

        public static OrderLine GetOrderLine(this ILineItem lineItem, bool includeProductAndImageUrl = false)
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

            // Prices should include tax?
            orderLine.UnitPrice = AmountHelper.GetAmount(lineItem.PlacedPrice);
            orderLine.TotalAmount = AmountHelper.GetAmount(lineItem.PlacedPrice * lineItem.Quantity);
            orderLine.TotalDiscountAmount = 0;
            orderLine.TotalTaxAmount = 0;
            orderLine.TaxRate = 0;

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
    }
}