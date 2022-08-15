using EPiServer.DataAnnotations;
using Foundation.Features.Home;
using Foundation.Features.MyAccount.OrderConfirmation;
using Foundation.Infrastructure;

namespace Foundation.Features.Checkout
{
    [ContentType(DisplayName = "Klarna Checkout Page",
        GUID = "46B7E4EF-D6C2-46AB-B54F-9B1E6C5DD461",
        Description = "Dedicated checkout page for Klarna Checkout",
        GroupName = GroupNames.Commerce,
        AvailableInEditMode = false)]
    [AvailableContentTypes(Include = new[] { typeof(OrderConfirmationPage) }, IncludeOn = new[] { typeof(HomePage) })]
    [ImageUrl("/icons/cms/pages/cms-icon-page-08.png")]
    public class KlarnaCheckoutPage : CheckoutPage
    {
    }
}