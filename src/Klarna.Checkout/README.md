# EPiServer Klarna Checkout integration

## Description

Klarna.Checkout is a library which helps to integrate [Klarna Checkout (KCO)](https://developers.klarna.com/documentation/klarna-checkout/) as the checkout solution for your EPiServer Commerce sites.
This library consists of two assemblies. Both are mandatory for a creating an integration between EPiServer and Klarna.

## Integration

![Klarna Checkout integration](/docs/images/klarna-checkout-integration.png?raw=true)

## Features

- Klarna.Checkout is the integration between EPiServer and the Klarna Checkout API (https://developers.klarna.com/api/#checkout-api-create-a-new-order)
- Klarna.Checkout.CommerceManager contains a usercontrol for the payment method configuration in Commerce Manager
- Handle pending orders / fraud check results
- Add order notes to track update flow
- Pick shipping option in KCO widget
- Style KCO widget to your liking
- Add an additional checkbox

## Payment process / checkout flow

- **Visitor visits checkout page** - Klarna order is created or updated
- **Klarna checkout widget is loaded (payment option)**
  - Visitor can enter shipping and payment information
  - Callback url's are called for updating information in EPiServer.
    - These also return data to Klarna in order to update order/lineitem totals and available shipping options
- **Visitor clicks 'Place order' button**
  - The [order validation](https://developers.klarna.com/documentation/klarna-checkout/integration-guide/render-the-checkout/validate-order) url is called in order to execute the last checks before finalizing the order. For example check stock, validate order totals and addresses to make sure all data is valid. If the data is not valid the user can be redirected or can be shown an error (still on the checkout page)
- **The order is created at Klarna**
- **Visitor is redirected to confirmation callback url**
  - Purchase order is created in EPiServer
- **Visitor is redirected to confirmation page**
- **optional - Klarna - fraud status notification** - When the Klarna order is pending, then a fraud status notification is sent to the configured notification URL (configured in Commerce Manager)
- **delayed - Receive a [push callback](https://developers.klarna.com/documentation/klarna-checkout/integration-guide/confirm-purchase/) from Klarna** - This notifies Epi that the order has been created in Klarna Order Management (usually within a few seconds). We check if a PurchaseOrder has been made in Epi, acknowledge the order in Klarna and update the merchant reference to make sure the Klarna order data is complete.

More information about the Klarna Checkout flow: https://developers.klarna.com/documentation/klarna-checkout/.

<details>
  <summary>Setup (click to expand)</summary>

Start by installing NuGet packages (use [NuGet](https://nuget.episerver.com/)):

    Install-Package Klarna.Checkout.v3

For the Commerce Manager site run the following package:

    Install-Package Klarna.Checkout.CommerceManager.v3

</details>

<details>
  <summary>Configure Commerce Manager (click to expand)</summary>
  
Login into Commerce Manager and open **Administration -> Order System -> Payments**. Then click **New** and in **Overview** tab fill:

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

Click OK in order to save the Payment for the first time. After saving, return to the payment and go to the parameters tab

- **Market**
  - Select the market you want to set up
  - This will reflect the selected markets from the **Markets** tab (after saving)
- **Klarna connection settings**
  - Username(\*) - provided by Klarna
  - Password(\*) - provided by Klarna
  - ApiUrl(\*) - provided by Klarna
    - See the Klarna documentation for the API endpoints: https://developers.klarna.com/api/#api-urls. Klarna API requires HTTPS.
- **Widget settings**
  - [Some widget styling settings](https://developers.klarna.com/documentation/klarna-checkout/integration-guide/render-the-checkout/extra-features)
  - Shipping details, see [same link](https://developers.klarna.com/documentation/klarna-checkout/integration-guide/render-the-checkout/extra-features)
  - Select shipping option in Klarna Checkout iFrame - Unless you want to have your own shipping options selector, set this to true
  - Allow separate shipping address - If true, the consumer can enter different billing and shipping addresses. Default: false
  - Date of birth mandatory - If true, the consumer cannot skip date of birth. Default: false
  - Title mandatory - If specified to false, title becomes optional. Only available for orders for country GB.
  - Show subtotal detail - If true, the Order Detail subtotals view is expanded. Default: false
  - Send shipping countries - sends available countries from the Epi country dictionary
  - Prefill addresses - send address information on order creation in Klarna (preferred shipping/billing address)
  - Send shipping options prior to filling addresses - send in available shipping options even if address is unknown
- **Klarna Widget additional checkbox**
  - [Another extra feature](https://developers.klarna.com/documentation/klarna-checkout/integration-guide/render-the-checkout/extra-features) which enables you to add a checkbox within the Klarna checkout iFrame
- **Merchant/callback URLs**
  - Checkout url (\*) - URL of merchant checkout page. Should be different than terms, confirmation and push URLs.
  - Terms url (\*) - URL of merchant terms and conditions. Should be different than checkout, confirmation and push URLs
  - Push url (\*) - URL that will be requested when an order is completed. Should be different than checkout and confirmation URLs
  - Notification/fraud url - URL for notifications on pending orders
  - Shipping option update url - URL for shipping option update - must be https
  - Address update url - URL for shipping, tax and purchase currency updates. Will be called on address changes -must be https
  - Order validation url - URL that will be requested for final merchant validation - must be https
  - Confirmation url (\*) - URL of merchant confirmation page. Should be different than checkout and confirmation URLs

The Klarna.Checkout package will replace `{orderGroupId}` in any of the urls with the id of the cart. Klarna does a similar thing, they will replace `{checkout.order.id}` with the actual klarna order id (for example on confirmation url below)

![Checkout payment method settings](/docs/screenshots/checkout-parameters.PNG?raw=true "Checkout payment method parameters")

**Note: If the parameters tab is empty (or gateway class is missing), make sure you have installed the commerce manager package (see above)**

**Taxes: If the line items prices already include sales tax - make sure that PricesIncludeTax is set to true. This can be configured per market in Episerver Commerce. Default is false.**

</details>

<details>
<summary>Creating and updating checkout order data (click to expand)</summary>

Every time the user visits the checkout page or changes his/her order, an api call to Klarna is executed. The api call ensures that Klarna has the most recent information needed to show the checkout iFrame. By default all properties should be set as required by Klarna. If you want to hook into the process and change some of the data that is being sent, you can provide an implementation of `ICheckoutOrderDataBuilder` to do so. The interface has a `Build` method, which is called after all default values are set. Below an example implementation of a DemoCheckoutOrderDataBuilder.

```csharp
public class DemoCheckoutOrderDataBuilder : ICheckoutOrderDataBuilder
{
    public CheckoutOrderData Build(CheckoutOrderData checkoutOrderData, ICart cart, CheckoutConfiguration checkoutConfiguration)
    {
        if (checkoutConfiguration.PrefillAddress)
        {
            // Try to parse address into dutch address lines
            if (checkoutOrderData.ShippingAddress.Country.Equals("NL"))
            {
                var dutchAddress = ConvertToDutchAddress(checkoutOrderData.ShippingAddress);
                checkoutOrderData.ShippingAddress = dutchAddress;
            }
        }
        return checkoutOrderData;
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
```

</details>

<details>
  <summary>Quicksilver demo site implementation (click to expand)</summary>

**Start page setting**

When running the demo code in this repository make sure to enable Klarna Checkout on the start page (Commerce tab).

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
The demo site implementation only supports selecting the shipping address in the Klarna Checkout iFrame. By default the first available shipping option will be selected. If you want to support switching shipping options you can look at what happens upon updating the cart (and check out [Suspend and Resume here](https://developers.klarna.com/documentation/klarna-checkout/javascript-api/)).

**API controller - Callback communication**

Read more about callback functionality in the next section. In the demo site, you can find the code in the controller `KlarnaCheckoutController.cs`.

**Load and display payment - QuickSilver**

- [\_KlarnaCheckout.cshtml](/demo/Sources/EPiServer.Reference.Commerce.Site/Views/Payment/_KlarnaCheckout.cshtml) - display Klarna Checkout method by rendering HTML snippet
- [\_KlarnaCheckoutConfirmation.cshtml](/demo/Sources/EPiServer.Reference.Commerce.Site/Views/Shared/_KlarnaCheckoutConfirmation.cshtml) - Klarna Checkout confirmation view
- [KlarnaCheckoutPaymentMethod.cs](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Payment/PaymentMethods/KlarnaCheckoutPaymentMethod.cs)

**Process payment - QuickSilver**

- Call `IKlarnaCheckoutService.CreateOrUpdateOrder` to create or update a new checkout order. In QuickSilver this is called in the CheckoutController and CartController.
- `KlarnaCheckoutConfirmation` in CheckoutController is called when visitor clicks the purchase button in the Klarna widget and order was successfully created. See Commerce Manager setup how to configure this URL. In this action, the purchase order in Episerver is created.

</details>

<details>
<summary>Klarna callbacks (click to expand)</summary>

During the checkout process Klarna trigger one of the following callbacks.

#### [Shipping optionupdate](https://developers.klarna.com/documentation/klarna-checkout/integration-guide/render-the-checkout/tax-shipping/)

If shipping options are available in the iFrame, after selecting a new shipping option Klarna will send information to this callback url. The information can be used to recalculate shipping costs/order totals.

```csharp
[Route("cart/{orderGroupId}/shippingoptionupdate")]
[AcceptVerbs("POST")]
[HttpPost]
[ResponseType(typeof(ShippingOptionUpdateResponse))]
public IHttpActionResult ShippingOptionUpdate(int orderGroupId, [FromBody]ShippingOptionUpdateRequest shippingOptionUpdateRequest)
{
    var cart = _orderRepository.Load<ICart>(orderGroupId);
    var response = _klarnaCheckoutService.UpdateShippingMethod(cart, shippingOptionUpdateRequest);
    return Ok(response);
}
```

#### [Address update](https://developers.klarna.com/api/#checkout-api-callbacks-address-update)

If an address has been updated in the iFrame, new address will be sent to the address update callback url. The information can be used to supply new shipping options and order totals.

```csharp
[Route("cart/{orderGroupId}/addressupdate")]
[AcceptVerbs("POST")]
[HttpPost]
[ResponseType(typeof(AddressUpdateResponse))]
public IHttpActionResult AddressUpdate(int orderGroupId, [FromBody]AddressUpdateRequest addressUpdateRequest)
{
    var cart = _orderRepository.Load<ICart>(orderGroupId);
    var response = _klarnaCheckoutService.UpdateAddress(cart, addressUpdateRequest);
    return Ok(response);
}
```

#### [Order validation](https://developers.klarna.com/documentation/klarna-checkout/integration-guide/render-the-checkout/validate-order)

Klarna will do a request to the [order validation callback url](https://developers.klarna.com/api/#checkout-api-callbacks-order-validation). Here you can check if a purchase order can be made. Think of checking stock, checking billing and shipping addresses and comparing the epi cart with the provided data from Klarna.
If **Require validate callback success** is set to **true** Klarna will only create an order if they receive an HTTP status 200 OK response.

```csharp
[Route("cart/{orderGroupId}/ordervalidation")]
[AcceptVerbs("POST")]
[HttpPost]
public IHttpActionResult OrderValidation(int orderGroupId, [FromBody]PatchedCheckoutOrderData checkoutData)
{
    var cart = _orderRepository.Load<ICart>(orderGroupId);

    // Validate cart lineitems
    var validationIssues = new Dictionary<ILineItem, ValidationIssue>();
    cart.ValidateOrRemoveLineItems((lineItem, validationIssue) =>
    {
        validationIssues.Add(lineItem, validationIssue);
    }, _lineItemValidator);

    if (validationIssues.Any())
    {
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.RedirectMethod);
        httpResponseMessage.Headers.Location = new Uri("http://klarna.localtest.me?redirect");
        return ResponseMessage(httpResponseMessage);
    }

    // Validate billing address if necessary (this is just an example)
    if (checkoutData.BillingAddress.PostalCode.Equals("94108-2704"))
    {
        var errorResult = new ErrorResult
        {
            ErrorType = ErrorType.address_error,
            ErrorText = "We don't allow postalcode 94108-2704"
        };
        return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, errorResult));
    }

    // Validate order amount, shipping address
    if (!_klarnaCheckoutService.ValidateOrder(cart, checkoutData))
    {
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.RedirectMethod);
        httpResponseMessage.Headers.Location = new Uri("http://klarna.localtest.me?redirect");
        return ResponseMessage(httpResponseMessage);
    }

    return Ok();
}
```

#### Fraud status

In Commerce Manager the notification URL can be configured. Klarna will call this URL for notifications for an orders that needs an additional review (fraud reasons). The IKlarnaService includes a method for handling fraud notifications. Below an example implementation.

```
[Route("cart/{orderGroupId}/fraud")]
[AcceptVerbs("POST")]
[HttpPost]
public IHttpActionResult FraudNotification(int orderGroupId, string klarna_order_id)
{
    var purchaseOrder = GetOrCreatePurchaseOrder(orderGroupId, klarna_order_id);
    if (purchaseOrder == null)
    {
        return NotFound();
    }

    var requestParams = Request.Content.ReadAsStringAsync().Result;
    if (!string.IsNullOrEmpty(requestParams))
    {
        var notification = JsonConvert.DeserializeObject<NotificationModel>(requestParams);
        _klarnaCheckoutService.FraudUpdate(notification);
    }
    return Ok();
}
```

When a payment needs an additional review, the payment in EPiServer is set to the status PENDING and the order to ONHOLD. When the fraud status callback URL is called and the payment is accepted the payment status will be set to PROCESSED and the order to ONHOLD. If the payment is rejected by Klarna the payment status is set to FAILED. An note is added to the order to notify the editor that a payment was rejected.
![Payment fraud rejected](/docs/screenshots/order-payment-fraud-rejected.png?raw=true "Payment fraud rejected")

</details>
<details>
<summary>Order notes (click to expand)</summary>

The KlarnaPaymentGateway save notes about payment updates at the order.
![Order notes](/docs/screenshots/order-notes.PNG?raw=true "Order notes")

</details>
<details>
  <summary>External Payment Methods & External Checkout (click to expand)</summary>
  
Klarna Checkout offers a wide variety of payment methods to cover the main needs of consumers in all markets, which all are included with a simple, single integration.
 
[Here's the full documentation](https://developers.klarna.com/documentation/klarna-checkout/external-payment-methods/) including supported payment and checkouts - we recommend reading through it thoroughly and then coming back here.

![Klarna Checkout External Payment Methods & External Checkouts](https://developers.klarna.com/static/KCO_external-payment-methods.png)

The most important thing to note is that you need to implement the backend integration for the external payment/checkout yourself. So for instance if you wanted to add PayPal you would have to create a redirect URL that has the processing logic for PayPal. Example: klarna.geta.no/processpaypall.

In your ICheckoutOrderDataBuilder implementation and the Build() method you would pass along the details of the payment method:

```
checkoutOrderData.ExternalPaymentMethods = new[]
{
    new ExternalPaymentMethod { Fee = 10, ImageUri = new Uri("https://klarna.geta.no/Styles/Images/paypal.png"), Name  = "PayPal", RedirectUri = new Uri("https://klarna.geta.no/processpaypall")}
};
```
Name is case sensitiv so make sure to check the supported name in the documentation and the URLs all have to be https.

You can find an [example in the demo site](https://github.com/Geta/Klarna/blob/aab444b0c2ce6c4319e808d4d2b203242ba3bbda/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Checkout/DemoCheckoutOrderDataBuilder.cs#L34).
</details>

## Local development environment

In order to use / work on this package locally you'll need a tool called www.ngrok.com. This tool can forward a generated ngrok URL to a localhost URL. Klarna Checkout will react on interactions in the widget by executing (push) URL's (configured in commerce manager). If Klarna can't successfully do these request it will show an error modal in the widget.

## Demo

https://kco-klarna.geta.no

## Package maintainer

[Brian Weeteling](https://github.com/brianweet)

## Changelog

[Changelog](../../CHANGELOG.md)
