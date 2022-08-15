# Optimizely Klarna Checkout integration

## Description

Klarna.Checkout is a library which helps to integrate [Klarna Checkout (KCO)](https://docs.klarna.com/klarna-checkout/) as the checkout solution for your Optimizely Commerce store.

## Integration

![Klarna Checkout integration](/docs/images/klarna-checkout-integration.png?raw=true)

## Features

- Klarna.Checkout is the integration between Optimizely and the Klarna Checkout API (https://developers.klarna.com/api/#checkout-api-create-a-new-order)
- Handle pending orders / fraud check results
- Add order notes to track update flow
- Pick shipping option in KCO widget
- Style KCO widget to your liking
- Add an additional checkbox
- Klarna Checkout configuration

## Payment process / checkout flow

- **Visitor visits checkout page** - Klarna order is created or updated
- **Klarna checkout widget is loaded (payment option)**
  - Visitor can enter shipping and payment information
  - Callback url's are called for updating information in Optimizely.
    - These also return data to Klarna in order to update order/lineitem totals and available shipping options
- **Visitor clicks 'Place order' button**
  - The [order validation](https://docs.klarna.com/klarna-checkout/popular-use-cases/validate-order/) url is called in order to execute the last checks before finalizing the order. For example check stock, validate order totals and addresses to make sure all data is valid. If the data is not valid the user can be redirected or can be shown an error message (still on the checkout page)

- **The order is created at Klarna**
- **Visitor is redirected to confirmation callback url**
  - The purchase order is created in Optimizely
- **Visitor is redirected to confirmation page**
- **optional - Klarna - fraud status notification** - When the Klarna order is pending, then a fraud status notification is sent to the configured notification URL (see configuration below)
- **delayed - Receive a [push callback](https://docs.klarna.com/klarna-checkout/in-depth-knowledge/confirm-purchase/) from Klarna** - This notifies Optimizely that the order has been created in Klarna Order Management (usually within a few seconds). We check if a Purchase Order has been made in Optimizely, acknowledge the order in Klarna and update the merchant reference to make sure the Klarna order data is complete.

More information about the Klarna Checkout flow: https://docs.klarna.com/klarna-checkout/.

<details>
  <summary>Setup (click to expand)</summary>

Start by installing NuGet package (use [NuGet](https://nuget.optimizely.com/))

```
    dotnet add package Klarna.Checkout.v3
```
	
In Startup.cs

```
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddKlarnaCheckout();
}
```

</details>

<details>
  <summary>Configure (click to expand)</summary>
  
Login into Optimizely with a CommerceAdmin user and go to **Commerce -> Administration -> Payments**. Then click **New** and in **Overview** tab fill:

(\*) mandatory

- Name(\*)
- System Keyword(\*) - KlarnaCheckout (the integration will not work when something else is entered in this field)
- Language(\*) - allows a specific language to be specified for the payment gateway
- Class Name(\*) - choose **Klarna.Checkout.KlarnaCheckoutGateway**
- Payment Class(\*) - choose **Mediachase.Commerce.Orders.OtherPayment**
- IsActive - **Yes**
- Supports Recurring - **No** - this Klarna Checkout integration does not support recurring payments
- Select shipping methods available for this payment
- Select markets available for this payment

Click OK in order to save the Payment for the first time.

** appsettings

Once you have created the payment method in the Commerce interface go to your stores appsettings.json file and add the configuration using the following convention **Klarna -> Checkout -> MarketId**.

Example:

```
"Klarna": {
    "Checkout": {
      "US": { // This is the market id
        ...
      }
	}
}
```

There are 7 properties that are required and several that are optional or that have default values that can be changed if needed.

For a developer test account see: https://docs.klarna.com/resources/test-environment/. 

*** Required properties ***

| Name      | Description |
| ----------- | ----------- |
| Username   | API username - provided by Klarna        |
| Password   | API password - provided by Klarna        |
| ApiUrl   | Base Url - See the Klarna documentation for the API endpoints: https://developers.klarna.com/api/#api-urls. Klarna API requires HTTPS.        |
| CheckoutUrl   | URL of merchant checkout page. Should be different then terms, confirmation and push URLs.        |
| TermsUrl   | URL for the terms and conditions page of the merchant. The URL will be displayed inside the Klarna Checkout iFrame.        |
| ConfirmationUrl   | URL of the merchant confirmation page. The consumer will be redirected back to the confirmation page if the authorization is successful after the customer clicks on the ‘Place Order’ button inside checkout.        |
| PushUrl   | URL that will be used for push notification when an order is completed. Should be different than checkout and confirmation URLs.        |

Example: 

```
"Klarna": {
    "Checkout": {
      "US": {
        "Username": "",
        "Password": "",
        "ApiUrl": "https://api-na.playground.klarna.com/",
        "CheckoutUrl": "/checkout",
        "ConfirmationUrl": "/checkout/KlarnaCheckoutConfirmation?orderGroupId={orderGroupId}&klarna_order_id={checkout.order.id}",
        "TermsUrl": "/terms",
        "PushUrl": "/klarnacheckout/cart/{orderGroupId}/push?klarna_order_id={checkout.order.id}",
      }
	}
}
```

*** Other properties ***

All URLs must be https.

| Name      | Description |
| ----------- | ----------- |
| WidgetButtonColor        | Color for the buttons within the iFrame. Value should be a CSS hex color, e.g. "#FF9900"       |
| WidgetButtonTextColor         | Color for the text inside the buttons within the iFrame. Value should be a CSS hex color, e.g. "#FF9900"       |
| WidgetCheckboxColor         | Color for the checkboxes within the iFrame. Value should be a CSS hex color, e.g. "#FF9900"       |
| WidgetCheckboxCheckmarkColor         | Color for the checkboxes checkmark within the iFrame. Value should be a CSS hex color, e.g. "#FF9900"       |
| WidgetHeaderColor         | Color for the headers within the iFrame. Value should be a CSS hex color, e.g. "#FF9900"       |
| WidgetLinkColor         | Color for the hyperlinks within the iFrame. Value should be a CSS hex color, e.g. "#FF9900"       |
| WidgetBorderRadius         | Radius for the border of elements within the iFrame. Example: 2px       |
| ShippingDetailsText         | A message that will be presented on the confirmation page under the headline "Delivery" (max 255 characters)       |
| ShippingOptionsInIFrame         | Select shipping option in Klarna Checkout iFrame - Unless you want to have your own shipping options selector, leave the default value of true       |
| AllowSeparateShippingAddress          | If true, the consumer can enter different billing and shipping addresses. Default: false, except for purchase_country DE where default is: true       |
| DateOfBirthMandatory          | Date of birth mandatory - If true, the consumer cannot skip date of birth. Default: false       |
| TitleMandatory          | Title mandatory - If specified to false, title becomes optional. Default: false. Only available for orders for country GB       |
| ShowSubtotalDetail          | Show subtotal detail - If true, the Order Detail subtotals view is expanded. Default: false       |
| AdditionalCheckboxText           | Additional checkbox text. Klarna Docs: [Custom checkboxes](https://docs.klarna.com/klarna-checkout/popular-use-cases/checkboxes/).       |
| AdditionalCheckboxDefaultChecked           | Additional checkbox default checked. True/false       |
| AdditionalCheckboxRequired           | Additional checkbox required. True/false       |
| SendShippingCountries           | Send shipping countries - sends available countries from the Optimizely country dictionary. True/false      |
| PrefillAddress           | Prefill addresses - send address information on order creation in Klarna (preferred shipping/billing address). True/false      |
| SendShippingOptionsPriorAddresses           | Send shipping options prior to filling addresses - send in available shipping options even if address is unknown. Default: true       |
| NotificationUrl           | URL for notifications on pending orders. (max 2000 characters). Example: "https://merchant.com/notification/{checkout.order.id}"        |
| CancellationTermsUrl           | URL for the cancellation terms page of the merchant. The URL will be displayed in the email that is sent to the customer after the order is captured       |
| ShippingOptionUpdateUrl           | Shipping option update url - URL for shipping option update       |
| AddressUpdateUrl            | Address update url - URL for shipping, tax and purchase currency updates. Will be called on address changes        |
| OrderValidationUrl            | Order validation url - URL that will be requested for final merchant validation       |
| RequireValidateCallbackSuccess            | Required validate callback success. Default: true       |
| SendProductAndImageUrl            | Send product and image URL to Klarna for each line item. Default: true       |

The Klarna.Checkout package will replace `{orderGroupId}` in any of the urls with the id of the cart. Klarna does a similar thing, they will replace `{checkout.order.id}` with the actual klarna order id (for example on confirmation url below).

![Checkout payment method settings](/docs/screenshots/checkout-parameters.PNG?raw=true "Checkout payment method parameters")

**Taxes: If the line items prices already include sales tax - make sure that PricesIncludeTax is set to true. This can be configured per market in Optimizely Commerce. Default is false.**

After you've added the appsettings configuration you need to configure the service your app Startup. The convention is to use the market id as name.

Example:

```csharp
private readonly IConfiguration _configuration;

public Startup(IWebHostEnvironment webHostingEnvironment, IConfiguration configuration)
{
	_webHostingEnvironment = webHostingEnvironment;
	_configuration = configuration;
}

public void ConfigureServices(IServiceCollection services)
{
	services.Configure<CheckoutConfiguration>("US", _configuration.GetSection("Klarna:Checkout:US")); // US is the market id
}
```

</details>

<details>
<summary>Creating and updating checkout order data (click to expand)</summary>

Every time the user visits the checkout page or changes his/her order, an api call to Klarna is executed. The api call ensures that Klarna has the most recent information needed to show the checkout iFrame. By default all properties should be set as required by Klarna. If you want to hook into the process and change some of the data that is being sent, you can provide an implementation of `ICheckoutOrderDataBuilder` to do so. The interface has a `Build` method, which is called after all default values are set. Below an example implementation of a DemoCheckoutOrderDataBuilder.

```csharp
public class DemoCheckoutOrderDataBuilder : ICheckoutOrderDataBuilder
{
    public CheckoutOrder Build(CheckoutOrder checkoutOrderData, ICart cart, CheckoutConfiguration checkoutConfiguration)
    {
        if (checkoutConfiguration.PrefillAddress)
        {
            // Try to parse address into dutch address lines
            if (checkoutOrderData.ShippingCheckoutAddress.Country.Equals("NL"))
            {
                var dutchAddress = ConvertToDutchAddress(checkoutOrderData.ShippingCheckoutAddress);
                checkoutOrderData.ShippingCheckoutAddress = dutchAddress;
            }
        }
        return checkoutOrderData;
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
```

</details>

<details>
  <summary>Foundation demo site implementation (click to expand)</summary>

**Default properties**

The following properties are set by default (read from current cart and payment method configurations):

- **PurchaseCountry**
- **MerchantUrl.Confirmation**
- **MerchantUrl.Notification**
- **Options**
- **OrderAmount**
- **PurchaseCurrency**
- **Locale**
- **OrderLines**
- **ShippingAddress**
- **BillingAddress**

Read more about the different parameters: https://developers.klarna.com/api/#checkout-api-create-a-new-order.

**Remark:**

The demo site implementation only supports selecting the shipping address in the Klarna Checkout iFrame. By default the first available shipping option will be selected. If you want to support switching shipping options you can look at what happens upon updating the cart (and check out [Suspend and Resume here](https://docs.klarna.com/klarna-checkout/in-depth-knowledge/client-side-events/#checkout-actions-suspend-and-resume)).

**API controller - Callback communication**

Read more about callback functionality in the next section. In the demo site, you can find the code in the controller `KlarnaCheckoutController.cs`.

**Load and display payment - Foundation**

- [\_KlarnaCheckout.cshtml](/demo/Foundation/src/Foundation/Features/Checkout/_KlarnaCheckoutPaymentMethod.cshtml) - display Klarna Checkout method by rendering the HTML snippet
- [\_KlarnaCheckoutConfirmation.cshtml](/demo/Foundation/src/Foundation/Features/MyAccount/OrderConfirmation/_KlarnaCheckoutConfirmation.cshtml) - Klarna Checkout confirmation view
- [KlarnaCheckoutPaymentOption.cs](/demo/Foundation/src/Foundation/Features/Checkout/Payments/KlarnaCheckoutPaymentOption.cs)

**Process payment - Foundation**

- Call `IKlarnaCheckoutService.CreateOrUpdateOrder` to create or update a new checkout order. In Foundation this is called in the CheckoutController and CartController. This is an async method that requires your controller to be async, you can also use AsyncHelper.RunSync() to call it synchronize.
- `KlarnaCheckoutConfirmation` in CheckoutController is called when visitor clicks the purchase button in the Klarna widget and order was successfully created. See the configuration section above on how to configure this URL. In this action, the purchase order in Optimizely is created.

</details>

<details>
<summary>Klarna callbacks (click to expand)</summary>

During the checkout process Klarna triggers one of the following callbacks.

#### [Shipping optionupdate](https://docs.klarna.com/klarna-checkout/in-depth-knowledge/tax-handling/)

If shipping options are available in the iFrame, after selecting a new shipping option Klarna will send information to this callback url. The information can be used to recalculate shipping costs/order totals.

```csharp
[Route("cart/{orderGroupId}/shippingoptionupdate")]
[HttpPost]
public ActionResult ShippingOptionUpdate(int orderGroupId, [FromBody] ShippingOptionUpdateRequest shippingOptionUpdateRequest)
{
	var cart = _orderRepository.Load<ICart>(orderGroupId);

	var response = _klarnaCheckoutService.UpdateShippingMethod(cart, shippingOptionUpdateRequest);

	return Ok(response);
}
```

#### [Address update](https://docs.klarna.com/klarna-checkout/in-depth-knowledge/server-side-callbacks/#how-its-done-address-update)

If an address has been updated in the iFrame, new address will be sent to the address update callback url. The information can be used to supply new shipping options and order totals.

```csharp
[Route("cart/{orderGroupId}/addressupdate")]
[HttpPost]
public ActionResult AddressUpdate(int orderGroupId, [FromBody] CallbackAddressUpdateRequest addressUpdateRequest)
{
	var cart = _orderRepository.Load<ICart>(orderGroupId);
	var response = _klarnaCheckoutService.UpdateAddress(cart, addressUpdateRequest);
	return Ok(response);
}
```

#### [Order validation](https://docs.klarna.com/klarna-checkout/popular-use-cases/validate-order/)

Klarna will do a request to the [order validation callback url](https://developers.klarna.com/api/#checkout-api-callbacks-order-validation). Here you can check if a purchase order can be made. Think of checking stock, checking billing and shipping addresses and comparing the Optimizely cart with the provided data from Klarna.
If **RequireValidateCallbackSuccess** is set to **true** Klarna will only create an order if they receive an HTTP status 200 OK response.

```csharp
[Route("cart/{orderGroupId}/ordervalidation")]
[HttpPost]
public ActionResult OrderValidation(int orderGroupId, [FromBody] CheckoutOrder checkoutData)
{
	// More information: https://docs.klarna.com/klarna-checkout/popular-use-cases/validate-order/

	var cart = _orderRepository.Load<ICart>(orderGroupId);

	// Validate cart lineitems
	var validationIssues = _cartService.ValidateCart(cart);
	if (validationIssues.Any())
	{
		// check validation issues and redirect to a page to display the error
		return Redirect("/en/error-pages/checkout-something-went-wrong/");
	}

	// Validate billing address if necessary (this is just an example)
	// To return an error like this you need require_validate_callback_success set to true
	if (checkoutData.BillingCheckoutAddress.PostalCode.Equals("94108-2704"))
	{
		var errorResult = new ErrorResult
		{
			ErrorType = ErrorType.address_error,
			ErrorText = "Can't ship to postalcode 94108-2704"
		};
		return BadRequest(errorResult);
	}

	// Validate order amount, shipping address
	if (!_klarnaCheckoutService.ValidateOrder(cart, checkoutData))
	{
		return Redirect("/en/error-pages/checkout-something-went-wrong/");
	}

	return Ok();
}
```

#### Fraud status

If NotificationUrl is configured, Klarna will call this URL for notifications for orders that needs additional review (fraud reasons). The IKlarnaService includes a method for handling fraud notifications. Below is an example implementation.

```csharp
[Route("fraud")]
[HttpPost]
public ActionResult FraudNotification(NotificationModel notification)
{
	_klarnaCheckoutService.FraudUpdate(notification);
	return Ok();
}
```

When a payment needs an additional review, the payment in Optimizely is set to the status PENDING and the order to ONHOLD. When the fraud status callback URL is called and the payment is accepted the payment status will be set to PROCESSED and the order to ONHOLD. If the payment is rejected by Klarna the payment status is set to FAILED. An note is added to the order to notify the editor that a payment was rejected.
![Payment fraud rejected](/docs/screenshots/order-payment-fraud-rejected.png?raw=true "Payment fraud rejected")

#### [Push url](https://developers.klarna.com/api/#checkout-api__create-a-new-ordermerchant_urls__push) is called by Klarna when an order is completed in order for Optimizely to acknowledge the order. In the example above the URL would be '/klarnacheckout/cart/{orderGroupId}/push?klarna_order_id={checkout.order.id}'. 

```csharp
[Route("cart/{orderGroupId}/push")]
[HttpPost]
public async Task<ActionResult> Push(int orderGroupId, string klarna_order_id)
{
	if (klarna_order_id == null)
	{
		return BadRequest();
	}

	var purchaseOrder = await GetOrCreatePurchaseOrder(orderGroupId, klarna_order_id).ConfigureAwait(false);
	if (purchaseOrder == null)
	{
		return NotFound();
	}

	// Update merchant reference
	await _klarnaCheckoutService.UpdateMerchantReference1(purchaseOrder).ConfigureAwait(false);

	// Acknowledge the order through the order management API
	await _klarnaCheckoutService.AcknowledgeOrder(purchaseOrder);

	return Ok();
}

private async Task<IPurchaseOrder> GetOrCreatePurchaseOrder(int orderGroupId, string klarnaOrderId)
{
	// Check if the order has been created already
	var purchaseOrder = _klarnaCheckoutService.GetPurchaseOrderByKlarnaOrderId(klarnaOrderId);
	if (purchaseOrder != null)
	{
		return purchaseOrder;
	}

	// Check if we still have a cart and can create an order
	var cart = _orderRepository.Load<ICart>(orderGroupId);
	var cartKlarnaOrderId = cart.Properties[Constants.KlarnaCheckoutOrderIdCartField]?.ToString();
	if (cartKlarnaOrderId == null || !cartKlarnaOrderId.Equals(klarnaOrderId))
	{
		return null;
	}

	var market = _marketService.GetMarket(cart.MarketId);
	var order = await _klarnaCheckoutService.GetOrder(klarnaOrderId, market).ConfigureAwait(false);
	if (!order.Status.Equals("checkout_complete"))
	{
		// Won't create order, Klarna checkout not complete
		return null;
	}
	purchaseOrder = await _checkoutService.CreatePurchaseOrderForKlarna(klarnaOrderId, order, cart).ConfigureAwait(false);
	return purchaseOrder;
}
```

</details>
<details>
<summary>Order notes (click to expand)</summary>

The KlarnaPaymentGateway adds notes about payment updates to the order.
![Order notes](/docs/screenshots/order-notes.PNG?raw=true "Order notes")

</details>
<details>
  <summary>External Payment Methods & External Checkout (click to expand)</summary>
  
Klarna Checkout offers a wide variety of payment methods to cover the main needs of consumers in all markets, which all are included with a simple, single integration.
 
[Here's the full documentation](https://docs.klarna.com/klarna-checkout/in-depth-knowledge/external-payment-methods/) including supported payment and checkouts - we recommend reading through it thoroughly and then coming back here.

![Klarna Checkout External Payment Methods & External Checkouts](https://developers.klarna.com/static/KCO_external-payment-methods.png)

The most important thing to note is that you need to implement the backend integration for the external payment/checkout yourself. So for instance if you wanted to add PayPal you would have to create a redirect URL that has the processing logic for PayPal. Example: klarna.getadigital.com/processpaypall.

In your ICheckoutOrderDataBuilder implementation and the Build() method you would pass along the details of the payment method:

```csharp
checkoutOrderData.ExternalPaymentMethods = new[]
{
    new PaymentProvider { Fee = 10, ImageUrl = "https://klarna.getadigital.com/Styles/Images/paypalpng", Name  = "PayPal", RedirectUrl = "https://klarna.getadigital.com"}
};
```
Name is case sensitiv so make sure to check the supported name in the documentation and the URLs all have to be https.

You can find an [example in the demo site](/demo/Foundation/src/Foundation/Features/Checkout/KlarnaDemoCheckoutOrderDataBuilder.cs).
</details>

## Local development environment

In order to use / work on this package locally you'll need a tool called www.ngrok.com. This tool can forward a generated ngrok URL to a localhost URL. Klarna Checkout will react on interactions in the widget by executing (push) URL's (configured in commerce manager). If Klarna can't successfully do these request it will show an error modal in the widget.

## Demo

https://klarna.getadigital.com

## Package maintainer

https://github.com/frederikvig

## Changelog

[Changelog](../../CHANGELOG.md)
