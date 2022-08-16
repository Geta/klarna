# Klarna Express Button

The Express button provides shoppers a fast and convenient way to buy with Klarna.

Add the Express button to your cart page to offer a quick and secure checkout option for your customers. With pre-filled data for all Klarna shoppers, you’ll offer a convenient shopping experience that customers expect, even if it’s their first time visiting your site.

You need to have Klarna Payments setup and the domain added to the allowed list with Klarna before being able to use the Express button, please see the [prerequisites](https://docs.klarna.com/express-button/prerequisites/) for more information.

## Installation

After installing the Klarna Payments package and configuring it (make sure that the merchant ID has been added to appSettings). Follow the integration steps here: https://docs.klarna.com/express-button/steps-to-integrate/.

The MID can be pulled from the settings and added based on the market like this.

```csharp
private readonly IConfigurationLoader _configurationLoader;
private readonly ICurrentMarket _currentMarket;

var paymentsConfiguration = _configurationLoader.GetPaymentsConfiguration(_currentMarket.GetCurrentMarket().MarketId); 
var mid = paymentsConfiguration.Mid;
```

## Sample code

Here's some sample code to get you started (using Foundation as example store). The basic flow is to show the button to the user in the mini cart (header) and on the cart page, allowing quick checkout using Klarna Payments as payment method. When the users clicks the button Klarna will sign in the user on their end and return the billing details (name, email, address, country etc).

For most cases (75%) for customers the billing and shipping address are the same so we can prefill both of these using this data in the checkout form (and pre-select Klarna Payments as payment option).

KlarnaExpressButtonViewModel.cs
```csharp
public class KlarnaExpressButtonViewModel
{
	public string Mid { get; set; }
	public string Locale { get; set; }
	public string Theme { get; set; }
}
```

We can then modify CartViewModelFactory.cs to include the KlarnaExpressButtonViewModel.

```csharp
if (cartPage.ShowKlarnaExpressButton)
{
	var paymentsConfiguration = _configurationLoader.GetPaymentsConfiguration(_currentMarket.GetCurrentMarket().MarketId);

	if (paymentsConfiguration != null)
	{
		klarnaExpressButton = new KlarnaExpressButtonViewModel { Mid = paymentsConfiguration.Mid, Locale = KlarnaHelper.GetLocale(), Theme = "light" };
	}
}
```

_ExpressButton.cshtml
```csharp
@model Foundation.Features.NamedCarts.DefaultCart.KlarnaExpressButtonViewModel

<script src="https://x.klarnacdn.net/express-button/v1/lib.js"
        data-id="@Model.Mid"
        data-environment="playground"
        async></script>

<div class="row">
    <div class="col-12">
        <klarna-express-button data-locale="@Model.Locale" data-theme="@Model.Theme" />
    </div>
</div>

<script>
    window.klarnaExpressButtonAsyncCallback = function () {
        Klarna.ExpressButton.on('user-authenticated', function (callbackData) {
            fetch('/klarnaapi/express/authenticated', {
                method: 'POST', 
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    "given_name": callbackData.first_name,
                    "family_name": callbackData.last_name,
                    "email": callbackData.email,
                    "phone": callbackData.phone,
                    "date_of_birth": callbackData.date_of_birth,
                    "street_address": callbackData.address.street_address,
                    "street_address2": callbackData.address.street_address2,
                    "postal_code": callbackData.address.postal_code,
                    "city": callbackData.address.city,
                    "region": callbackData.address.region,
                    "country": callbackData.address.country
                })
            }).then(response => {
                window.location.href = '/kp'; // redirect to Klarna Payments checkout page after updating the customers cart server side with new shipping and billing address
            }).catch(function(err) {
                console.info(err);
            });
        });
    }
</script>
```

Server side we have an endpoint that will take the data and update the cart with the address info.

KlarnaPaymentsApiController.cs
```csharp
[Route("express/authenticated")]
[HttpPost]
public ActionResult ExpressButtonAuthenticated([FromBody] OrderManagementAddressInfo addressInfo)
{
	if (addressInfo == null || string.IsNullOrEmpty(addressInfo.GivenName) || string.IsNullOrEmpty(addressInfo.FamilyName))
	{
		return BadRequest();
	}

	var cart = _cartService.LoadCart(_cartService.DefaultCartName, false);
	var shipment = cart.Cart.GetFirstShipment();
	var payment = cart.Cart.GetFirstForm()?.Payments.FirstOrDefault();

	var addressModel = new AddressModel
	{
		Name = $"{addressInfo.GivenName} {addressInfo.FamilyName} {DateTime.Now}",
		FirstName = addressInfo.GivenName,
		LastName = addressInfo.FamilyName,
		Line1 = addressInfo.StreetAddress,
		Line2 = addressInfo.StreetAddress2,
		City = addressInfo.City,
		PostalCode = addressInfo.PostalCode,
		CountryRegion = new CountryRegionViewModel {Region = addressInfo.Region},
		CountryCode = addressInfo.Country,
		Email = addressInfo.Email,
		DaytimePhoneNumber = addressInfo.Phone
	};

	var orderAddress = _addressBookService.ConvertToAddress(addressModel, cart.Cart);

	if (shipment != null)
	{
		shipment.ShippingAddress = orderAddress;
	}

	if (payment != null)
	{
		payment.BillingAddress = orderAddress;
	}

	_orderRepository.Save(cart.Cart);

	return Ok();
}
```
