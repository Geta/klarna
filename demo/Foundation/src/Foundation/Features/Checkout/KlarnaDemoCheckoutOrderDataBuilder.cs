using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Foundation.Features.CatalogContent.Product;
using Foundation.Infrastructure.Commerce.Extensions;
using Klarna.Checkout;
using Klarna.Checkout.Models;
using Klarna.Common.Configuration;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Mediachase.Commerce.Catalog;

namespace Foundation.Features.Checkout
{
	//[ServiceConfiguration(typeof(ICheckoutOrderDataBuilder), Lifecycle = ServiceInstanceScope.Transient)]
    public class KlarnaDemoCheckoutOrderDataBuilder : ICheckoutOrderDataBuilder
    {
        private Injected<UrlResolver> _urlResolver = default(Injected<UrlResolver>);
        private Injected<IContentRepository> _contentRepository = default(Injected<IContentRepository>);
        private Injected<ReferenceConverter> _referenceConverter = default(Injected<ReferenceConverter>);
        private Injected<IRelationRepository> _relationRepository = default(Injected<IRelationRepository>);

        public CheckoutOrder Build(CheckoutOrder checkoutOrderData, ICart cart, CheckoutConfiguration checkoutConfiguration)
        {
            if (checkoutConfiguration == null)
                return checkoutOrderData;

            checkoutOrderData.ExternalPaymentMethods = new[]
            {
                new PaymentProvider { Fee = 10, ImageUrl = "https://klarna.getadigital.com/Styles/Images/paypalpng", Name  = "PayPal", RedirectUrl = "https://klarna.getadigital.com"}
            };

            if (checkoutConfiguration.PrefillAddress)
            {
                // Try to parse address into dutch address lines
                if (checkoutOrderData.ShippingCheckoutAddress != null && !string.IsNullOrEmpty(checkoutOrderData.ShippingCheckoutAddress.Country) && checkoutOrderData.ShippingCheckoutAddress.Country.Equals("NL"))
                {
                    var dutchAddress = ConvertToDutchAddress(checkoutOrderData.ShippingCheckoutAddress);
                    checkoutOrderData.ShippingCheckoutAddress = dutchAddress;
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

        private void UpdateOrderLines(ICollection<OrderLine> orderLines, CheckoutConfiguration checkoutConfiguration)
        {
            foreach (var lineItem in orderLines)
            {
                if (lineItem != null && lineItem.Type.Equals("physical"))
                {
                    EntryContentBase entryContent = null;
                    GenericProduct product = null;
                    if (!string.IsNullOrEmpty(lineItem.Reference))
                    {
                        var contentLink = _referenceConverter.Service.GetContentLink(lineItem.Reference);
                        if (!ContentReference.IsNullOrEmpty(contentLink))
                        {
                            entryContent = _contentRepository.Service.Get<EntryContentBase>(contentLink);

                            var parentLink =
                                entryContent.GetParentProducts(_relationRepository.Service).SingleOrDefault();

                            _contentRepository.Service.TryGet<GenericProduct>(parentLink, out product);
                        }
                    }

                    if (lineItem.ProductIdentifiers == null)
                    {
                        lineItem.ProductIdentifiers = new ProductIdentifiers();
                    }

                    lineItem.ProductIdentifiers.Brand = product?.Brand;
                    lineItem.ProductIdentifiers.GlobalTradeItemNumber = "GlobalTradeItemNumber test";
                    lineItem.ProductIdentifiers.ManufacturerPartNumber = "ManuFacturerPartNumber test";
                    lineItem.ProductIdentifiers.CategoryPath = "test / test";

                    if (checkoutConfiguration.SendProductAndImageUrl && entryContent != null)
                    {
                        lineItem.ProductUrl = SiteUrlHelper.GetAbsoluteUrl()
                                                                  + entryContent.GetUrl(_relationRepository.Service, _urlResolver.Service);
                    }
                }
            }
        }

        private CheckoutAddressInfo ConvertToDutchAddress(CheckoutAddressInfo address)
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