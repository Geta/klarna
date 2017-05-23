EPiServer Klarna checkout integration
=============

## What is Klarna.Checkout?

Klarna.Checkout is a library which helps to integrate Klarna Checkout as one of the payment options in your EPiServer Commerce sites.
This library consists of two assemblies. Both are mandatory for a creating an integration between EPiServer and Klarna: 
* Klarna.Checkout is the integration between EPiServer and the Klarna Checkout API (https://developers.klarna.com/api/#checkout-api-order)
* Klarna.Checkout.CommerceManager contains a usercontrol for the payment method configuration in Commerce Manager

## Payment process / checkout flow
  
- **Visitor visits checkout page** - Klarna order is created or updated 
- **Klarna checkout widget is loaded (payment option)**  
    - Visitor can enter shipping and payment information
    - Callback url's are called for updating information in EPiServer. 
        - These also return data to Klarna in order to update order/lineitem totals and available shipping options
- **Visitor clicks 'Place order' button**
    - The [order validation](https://developers.klarna.com/en/us/kco-v3/checkout/additional-features/validate-an-order) url is called in order to execute the last checks before finalizing the order. For example check stock, validate order totals and addresses to make sure all data is valid. If the data is not valid the user can be redirected or can be shown an error (still on the checkout page)
- **The order is created at Klarna**
- **Visitor is redirected to confirmation callback url**
    - Purchase order is created in EPiServer
- **Visitor is redirected to confirmation page**
- **optional - Klarna - fraud status notification** - When the Klarna order is pending then fraud status notification are send to the configured notification URL (configured in Commerce Manager)
- **delayed - Receive a [push callback](https://developers.klarna.com/en/us/kco-v3/checkout/4-confirm-purchase) from Klarna** - This notifies Epi that the order has been created in Klarna Order Management (usually within a few seconds). We check if a PurchaseOrder has been made in Epi, acknowledge the order in Klarna and update the merchant reference to make sure the Klarna order data is complete.

More information about the Klarna Checkout flow: https://developers.klarna.com/en/gb/kco-v3/checkout

## Setup

Start by installing NuGet packages (use [NuGet](http://nuget.episerver.com/)):

    Install-Package Klarna.Checkout

For the Commerce Manager site run the following package:

    Install-Package Klarna.Checkout.CommerceManager


<details>
  <summary>Configure Commerce Manager (click to expand)</summary>
  
Login into Commerce Manager and open **Administration -> Order System -> Payments**. Then click **New** and in **Overview** tab fill:

- Name(*)
- System Keyword(*) - KlarnaCheckout (the integration will not work when something else is entered in this field)
- Language(*) - allows a specific language to be specified for the payment gateway
- Class Name(*) - choose **Klarna.Checkout.KlarnaCheckoutGateway**
- Payment Class(*) - choose **Mediachase.Commerce.Orders.OtherPayment**
- IsActive - **Yes**

(*) mandatory
- select shipping methods available for this payment

![Payment method settings](/docs/screenshots/payment-overview.PNG?raw=true "Payment method settings")

- navigate to parameters tab and fill in settings (see screenshot below)

**Connection string**

Connection string configurations for the connection with the Klarna APi. See the Klarna documentation for the API endpoints: https://developers.klarna.com/api/#api-urls. Klarna API requires HTTPS.

**Widget settings**

Set the colors and border size for the Klarna widget. The Klarna logo should be placed by the developer somewhere on the checkout/payment page.

**Other settings**

After payment is completed the confirmation url must be called. This can be done with this method:
```
_klarnaService.RedirectToConfirmationUrl(purchaseOrder);
```
Notification url is called by Klarna for fraud updates. See further in the documentation for an example implementation. 


![Payment method settings](/docs/screenshots/payment-parameters.PNG?raw=true "Payment method parameters")

**Note: If the parameters tab is empty (or gateway class is missing), make sure you have installed the commerce manager nuget (see above)**

- In the **Markets** tab select a market for which this payment will be available.
  
</details>

<details>
  <summary>Quicksilver demo site implementation (click to expand)</summary>

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

Read more about the different parameters: https://developers.klarna.com/api/#payments-api-create-a-new-session.
</details>

<details>
<summary>Callbacks</summary>
During the checkout process Klarna trigger one of the following callbacks.

#### [Shipping optionupdate](https://developers.klarna.com/en/us/kco-v3/checkout/additional-features/tax-shipping)
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

#### [Order validation](https://developers.klarna.com/en/us/kco-v3/checkout/additional-features/validate-an-order)
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

</details>
<details>
<summary>Callbacks</summary>

The KlarnaPaymentGateway save notes about payment updates at the order.
![Order notes](/docs/screenshots/order-notes.PNG?raw=true "Order notes")
</details>
