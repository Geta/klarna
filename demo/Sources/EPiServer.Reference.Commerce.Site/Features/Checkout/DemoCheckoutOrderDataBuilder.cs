using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Klarna.Checkout;
using Klarna.Checkout.Models;
using Klarna.Common.Models;
using Klarna.Rest.Models;
using Mediachase.Commerce.Catalog;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout
{
    public class DemoCheckoutOrderDataBuilder : ICheckoutOrderDataBuilder
    {
        private Injected<ILinksRepository> _linksRepository = default(Injected<ILinksRepository>);
        private Injected<UrlResolver> _urlResolver = default(Injected<UrlResolver>);
        private Injected<IContentRepository> _contentRepository = default(Injected<IContentRepository>);
        private Injected<ReferenceConverter> _referenceConverter = default(Injected<ReferenceConverter>);
        private Injected<IRelationRepository> _relationRepository = default(Injected<IRelationRepository>);
        
        public CheckoutOrderData Build(CheckoutOrderData checkoutOrderData, ICart cart, CheckoutConfiguration checkoutConfiguration)
        {
            if (checkoutConfiguration == null)
                return checkoutOrderData;

            if (checkoutConfiguration.PrefillAddress)
            {
                // Try to parse address into dutch address lines
                if (checkoutOrderData.ShippingAddress != null && !string.IsNullOrEmpty(checkoutOrderData.ShippingAddress.Country) && checkoutOrderData.ShippingAddress.Country.Equals("NL"))
                {
                    var dutchAddress = ConvertToDutchAddress(checkoutOrderData.ShippingAddress);
                    checkoutOrderData.ShippingAddress = dutchAddress;
                }
            }
            UpdateOrderLines(checkoutOrderData.OrderLines, checkoutConfiguration);
            
            return checkoutOrderData;
        }

        public AddressUpdateResponse Build(AddressUpdateResponse addressUpdateResponse, ICart cart, CheckoutConfiguration checkoutConfiguration)
        {
            UpdateOrderLines(addressUpdateResponse.OrderLines, checkoutConfiguration);

            return addressUpdateResponse;
        }

        public ShippingOptionUpdateResponse Build(ShippingOptionUpdateResponse addressUpdateResponse, ICart cart, CheckoutConfiguration checkoutConfiguration)
        {
            UpdateOrderLines(addressUpdateResponse.OrderLines, checkoutConfiguration);

            return addressUpdateResponse;
        }

        private void UpdateOrderLines(IList<OrderLine> orderLines, CheckoutConfiguration checkoutConfiguration)
        {
            foreach (var lineItem in orderLines)
            {
                if (lineItem != null && lineItem.Type.Equals("physical"))
                {
                    EntryContentBase entryContent = null;
                    FashionProduct product = null;
                    if (!string.IsNullOrEmpty(lineItem.Reference))
                    {
                        var contentLink = _referenceConverter.Service.GetContentLink(lineItem.Reference);
                        if (!ContentReference.IsNullOrEmpty(contentLink))
                        {
                            entryContent = _contentRepository.Service.Get<EntryContentBase>(contentLink);

                            var parentLink =
                                entryContent.GetParentProducts(_relationRepository.Service).SingleOrDefault();
                            product = _contentRepository.Service.Get<FashionProduct>(parentLink);
                        }
                    }

                    var patchedOrderLine = (PatchedOrderLine)lineItem;
                    if (patchedOrderLine.ProductIdentifiers == null)
                    {
                        patchedOrderLine.ProductIdentifiers = new PatchedProductIdentifiers();
                    }


                    patchedOrderLine.ProductIdentifiers.Brand = product?.Brand;
                    patchedOrderLine.ProductIdentifiers.GlobalTradeItemNumber = "GlobalTradeItemNumber test";
                    patchedOrderLine.ProductIdentifiers.ManuFacturerPartNumber = "ManuFacturerPartNumber test";
                    patchedOrderLine.ProductIdentifiers.CategoryPath = "test / test";

                    if (checkoutConfiguration.SendProductAndImageUrl && entryContent != null)
                    {
                        ((PatchedOrderLine)lineItem).ProductUrl = entryContent.GetUrl(_linksRepository.Service,
                            _urlResolver.Service);

                    }
                }
            }
        }

        private Address ConvertToDutchAddress(Address address)
        {
            // Just an example, do not use

            var splitAddress = address.StreetAddress.Split(' ');
            address.StreetName = splitAddress.FirstOrDefault();
            address.StreetNumber = splitAddress.ElementAtOrDefault(1);
            
            address.StreetAddress = string.Empty;
            address.StreetAddress2 = string.Empty;

            return address;
        }
    }
}