using EPiServer.DataAnnotations;
using Foundation.Features.Home;
using Foundation.Features.MyAccount.OrderConfirmation;
using Foundation.Infrastructure;

namespace Foundation.Features.Checkout
{
    [ContentType(DisplayName = "Klarna Payments Page",
        GUID = "F85ABD3C-2F0F-4212-8F09-89B4FC23C593",
        Description = "Dedicated checkout page for Klarna Payments",
        GroupName = GroupNames.Commerce,
        AvailableInEditMode = false)]
    [AvailableContentTypes(Include = new[] { typeof(OrderConfirmationPage) }, IncludeOn = new[] { typeof(HomePage) })]
    [ImageUrl("/icons/cms/pages/cms-icon-page-08.png")]
    public class KlarnaPaymentsPage : CheckoutPage
    {
    }
}