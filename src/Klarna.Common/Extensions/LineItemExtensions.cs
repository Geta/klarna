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

namespace Klarna.Common.Extensions
{
    public static class LineItemExtensions
    {
        private static Injected<ReferenceConverter> _referenceConverter;
        private static Injected<UrlResolver> _urlResolver;
        private static Injected<IContentRepository> _contentRepository;

        public static PatchedOrderLine GetOrderLine(this ILineItem item, Currency currency, bool includeProductAndImageUrl = false)
        {

            var orderLine = new PatchedOrderLine();
            orderLine.Quantity = (int)item.Quantity;
            orderLine.Name = item.DisplayName;
            if (string.IsNullOrEmpty(orderLine.Name))
            {
                var entry = item.GetEntryContent();
                if (entry != null)
                {
                    orderLine.Name = entry.DisplayName;
                }
            }

            var unitPrice = item.PlacedPrice;
            var totalPrice = unitPrice * item.Quantity;
            var extendedPrice = item.GetExtendedPrice(currency).Amount;

            orderLine.Reference = item.Code;
            orderLine.UnitPrice = AmountHelper.GetAmount(unitPrice);
            orderLine.TotalDiscountAmount = AmountHelper.GetAmount(totalPrice - extendedPrice);
            orderLine.TotalAmount = AmountHelper.GetAmount(extendedPrice);
            orderLine.Type = "physical";

            if (includeProductAndImageUrl)
            {
                var contentLink = _referenceConverter.Service.GetContentLink(item.Code);
                if (!ContentReference.IsNullOrEmpty(contentLink))
                {
                    orderLine.ProductUrl = _urlResolver.Service.GetUrl(contentLink);
                    orderLine.ProductImageUrl = GetVariantImage(contentLink);
                }
            }
            return orderLine;
        }

        private static string GetVariantImage(ContentReference contentReference)
        {
            VariationContent variant;
            if (_contentRepository.Service.TryGet(contentReference, out variant))
            {
                return variant.CommerceMediaCollection.Select(media => _urlResolver.Service.GetUrl((ContentReference) media.AssetLink))
                    .FirstOrDefault();
            }
            return string.Empty;
        }
    }
}