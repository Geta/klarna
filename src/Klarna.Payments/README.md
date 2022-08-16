# Optimizely Klarna Payments integration

## Description

Klarna.Payments is a library which helps to integrate Klarna Payments as one of the payment options in your Optimizely Commerce store. The library uses the [Klarna Payments API](https://developers.klarna.com/api/#payments-api) to integrate with Optimizely.

## Integration

![Klarna Payments integration](https://github.com/Geta/Klarna/raw/master/docs/images/klarna-payments-integration.png)

## Features

- Loading Klarna Payments widget
- Cancel payments
- Returns
- Multi shipments
- Fraud checks & notifications
- Payment step history saved on order notes
- Klarna Payments configuration

### Payment process

- **Customer visits checkout page** - Klarna session is created or updated
- **Klarna widget is loaded (payment option)**
- **Customer presses the Purchase button** - Klarna payment authorize is called client-side
- **Klarna: Create order** - When payment authorization is completed then create order (payment gateway) at Klarna
- **Optimizely: Create purchase order** - Create purchase order in Optimizely
- **Klarna - fraud status notification** - When the Klarna order is pending then fraud status notification are sent to the configured notification URL

More information about the Klarna Payments flow: https://docs.klarna.com/klarna-payments/.

<details>
  <summary>Setup (click to expand)</summary>

Start by installing the NuGet package (use [NuGet](https://nuget.optimizely.com/))

```
dotnet add package Klarna.Payments.v3
```

In Startup.cs

```
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddKlarnaPayments();
}
```

</details>
<details>
  <summary>Configure (click to expand)</summary>

Login into Optimizely with a CommerceAdmin user and go to **Commerce -> Administration -> Payments**. Then click **New** and in **Overview** tab fill:

- **Name(\*)**
- **System Keyword(\*)** - KlarnaPayments (the integration will not work when something else is entered in this field)
- **Language(\*)** - allows a specific language to be specified for the payment gateway
- **Class Name(\*)** - choose **Klarna.Payments.KlarnaPaymentGateway**
- **Payment Class(\*)** - choose **Mediachase.Commerce.Orders.OtherPayment**
- **IsActive** - **Yes**
- **Supports Recurring** - **No** - this Klarna Payments integration does not support recurring payments

(\*) mandatory

- select shipping methods available for this payment

![Payment method settings](/docs/screenshots/payment-overview.PNG?raw=true "Payment method settings")

## Configuration

Once you have created the payment method in the Commerce interface, go to your stores appsettings.json file and add the configuration using the following convention **Klarna -> Payments -> MarketId**.

Example:

```
"Klarna": {
    "Payments": {
      "US": { // This is the market id
        ...
      }
    }
}
```

There are 6 properties that are required and several others that are optional or that have default values that can be changed if needed.

For a developer test account see: https://docs.klarna.com/resources/test-environment/. 

### Required properties

| Name      | Description |
| ----------- | ----------- |
| Mid      | Merchant ID - Get this from the Klarna Merchant Portal       |
| Username   | API username - provided by Klarna        |
| Password   | API password - provided by Klarna        |
| ApiUrl   | Base Url - See the Klarna documentation for the API endpoints: https://developers.klarna.com/api/#api-urls. Klarna API requires HTTPS.        |
| ConfirmationUrl   | URL of the merchant confirmation page. The consumer will be redirected back to the confirmation page if the consumer is sent to the redirect URL after placing the order. Insert {session.id} and/or {order.id} as placeholder to connect either of those IDs to the URL        |
| NotificationUrl   | URL for notifications on pending orders. Insert {session.id} and/or {order.id} as placeholder to connect either of those IDs to the URL        |
| PushUrl   | URL that will be requested when an order is completed. Should be different than checkout and confirmation URLs. Insert {session.id} and/or {order.id} as placeholder to connect either of those IDs to the URL        |

Example: 

```
"Klarna": {
    "Payments": {
      "US": {
        "Mid": "",
        "Username": "",
        "Password": "",
        "ApiUrl": "https://api-na.playground.klarna.com/",
        "ConfirmationUrl": "/klarnaapi/order/confirmation/",
        "NotificationUrl": "/klarnaapi/fraud",
        "PushUrl": "/klarnaapi/push?klarna_order_id={order.id}"
      }
	}
}
```

### Other properties

| Name      | Description |
| ----------- | ----------- |
| WidgetDetailsColor       | Color for the bullet points within the iFrame. Value should be a CSS hex color, e.g. "#FF9900"       |
| WidgetBorderColor        | Color for the border of elements within the iFrame. Value should be a CSS hex color, e.g. "#FF9900"       |
| WidgetSelectedBorderColor         | Color for the border of elements within the iFrame when selected by the customer. Value should be a CSS hex color, e.g. "#FF9900"       |
| WidgetTextColor         | Color for the texts within the iFrame. Value should be a CSS hex color, e.g. "#FF9900"       |
| WidgetBorderRadius          | Radius for the border of elements within the iFrame. Example: 2px       |
| Design         | Design package to use in the session. This can only be used if a custom design has been implemented for Klarna Payments and agreed upon in the agreement. It might have a financial impact. Klarna Delivery manager will provide the value for the parameter.        |
| SendProductAndImageUrl         | Send product and image URL to Klarna for each line item. true/false. Default: true      |
| UseAttachments          | If true extra data can be shared with Klarna. See example: DemoSessionBuilder.cs in demo site.       |
| CustomerPreAssessment          | If not set, customer data will not be sent to Klarna before making a purchase. Default: false       |
| AutoCapture          | Allow merchant to trigger auto capturing. Default: false       |

After you've added the appsettings configuration you need to add it under ConfigureServices in Startup.cs. The convention is to use the Market ID as the name.

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
	services.Configure<PaymentsConfiguration>("US", _configuration.GetSection("Klarna:Payments:US")); // US is the market id
}
```

After payment is completed (in Foundation this would be in the CheckoutController and PlaceOrder method), the [confirmation url](https://developers.klarna.com/api/#payments-api__create-a-new-credit-sessionmerchant_urls__confirmation) must be called. This can be done like this:

```csharp
var result = _klarnaPaymentsService.Complete(purchaseOrder);
	
if (result.IsRedirect)
{
    return Redirect(result.RedirectUrl);
}
```

This will then redirect to what you configured under ConfirmationUrl above. In Foundation we have the following code:

```csharp
[Route("klarnaapi")]
public class KlarnaPaymentsApiController : Controller
{
	[Route("order/confirmation")]
	public ActionResult OrderConfirmation(string orderNumber)
	{
		var purchaseOrder = _purchaseOrderRepository.Load(orderNumber);

		if (purchaseOrder == null)
		{
			return NotFound();
		}

		return Redirect(_checkoutService.BuildRedirectionUrl());
	}
}
```

[Notification url](https://developers.klarna.com/api/#payments-api__create-a-new-credit-sessionmerchant_urls__notification) is called by Klarna for fraud updates. In the example above the URL would be '/klarnaapi/fraud'. 

```csharp
[Route("klarnaapi")]
public class KlarnaPaymentsApiController : Controller
{
	[Route("fraud")]
	[HttpPost]
	public ActionResult FraudNotification(NotificationModel notification)
	{
		_klarnaPaymentsService.FraudUpdate(notification);
		return Ok();
	}
}
```

When a payment needs an additional review, the payment in Optimizely is set to the status PENDING and the order to ONHOLD. When the fraud status callback URL is called and the payment is accepted the payment status will be set to PROCESSED and the order to ONHOLD. If the payment is rejected by Klarna the payment status is set to FAILED.

[Push url](https://developers.klarna.com/api/#payments-api__create-a-new-credit-sessionmerchant_urls__push) is called by Klarna when an order is completed in order for Optimizely to acknowledge the order. In the example above the URL would be '/klarnaapi/push?klarna_order_id={order.id}'. 

```csharp
[Route("klarnaapi")]
public class KlarnaPaymentsApiController : Controller
{
	[Route("push")]
	[HttpPost]
	public async Task<ActionResult> Push(string klarna_order_id)
	{
		if (klarna_order_id == null)
		{
			return BadRequest();
		}

		var purchaseOrder = _klarnaPaymentsService.GetPurchaseOrderByKlarnaOrderId(klarna_order_id);
		if (purchaseOrder == null)
		{
			return NotFound();
		}

		// Acknowledge the order through the order management API
		await _klarnaPaymentsService.AcknowledgeOrder(purchaseOrder);

		return Ok();
	}
}
```

The `SendProductAndImageUrl` property indicates if the product (in cart) page and image URL should be sent to Klarna and displayed in the Merchant Portal. When the `UseAttachments` property is set to true - the developer should send extra information to Klarna. See the [Klarna documentation](https://developers.klarna.com/documentation/klarna-payments/integration-guide/create-session/#extra-merchant-data) for more detailed explanation.

The `Pre-assesment` field indicates if customer information should be sent to Klarna prior to authorization. Klarna will review this information to verify if the customer can buy via Klarna. This option is only available in the U.S. market and will be ignored for all other markets. Below is a code snippet for sending customer information. An implementation of the ISessionBuilder can be used for setting this information. The ISessionBuilder interface is explained later in this document.

```chsarp
sessionRequest.Customer = new Customer
{
    DateOfBirth = "1980-01-01",
    Gender = "Male",
    LastFourSsn = "1234"
};
```

**Taxes: If the line items prices already include sales tax - make sure that PricesIncludeTax is set to true. This can be configured per market in Optimizely Commerce. Default is false.**

- In the **Markets** tab select a market for which this payment will be available.
  </details>

<details>
  <summary>Creating session (click to expand)</summary>

A session at Klarna should be created when the visitor is on the checkout page. The CreateOrUpdateSession method will create a new session when it does not exist, or update the current one. Use the SessionSettings object and the AdditionalValues property (IDictionary<string, object>) to pass extra data that can be used in the session builder.

Example:

```csharp
await _klarnaPaymentsService.CreateOrUpdateSession(MyCart, new SessionSettings(SiteDefinition.Current.SiteUrl));
```

It's possible to create an implementation of the ISessionBuilder. The Build method is called after all default values are set. This way you're able to override existing values or set missing ones. MerchantReference1 is used for the Purchase Order Number from Optimizely, MerchantReference2 can be used for additional data for that order which the merchant can then use to search and locate that particular order in the Klarna Portal (see example below in DemoSessionBuilder). The `includePersonalInformation` parameter indicates if personal information can be sent to Klarna. There are some restrictions for certain countries. For example, countries in the EU can only send personal information once customer has actively selected a Klarna payment method. For more details on legal & privacy [see here](https://developers.klarna.com/documentation/klarna-payments/legal-privacy/). 

You can add additional merchant data like customer data, subscription, event, reservation details etc when `UseAttachments` is set to true (see configuration above). [Here's a list](https://developers.klarna.com/api/#payments-api-create-a-new-credit-session) of all the different supported parameters. 

Below is an example implementation of ISessionBuilder.

```csharp
public class DemoSessionBuilder : ISessionBuilder
{
        public Session Build(Session session, ICart cart, PaymentsConfiguration configuration, IDictionary<string, object> dic = null, bool includePersonalInformation = false)
    {
        if (includePersonalInformation && paymentsConfiguration.CustomerPreAssessment)
        {
            session.Customer = new Customer
            {
                DateOfBirth = "1980-01-01",
                Gender = "Male",
                LastFourSsn = "1234"
            };
        }
        session.MerchantReference2 = "12345";

        if (paymentsConfiguration.UseAttachments && PrincipalInfo.CurrentPrincipal.Identity.IsAuthenticated)
		{
			var converter = new IsoDateTimeConverter
			{
				DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"
			};

			var customerContact = PrincipalInfo.CurrentPrincipal.GetCustomerContact();

			var customerAccountInfos = new List<Dictionary<string, object>>
				{
					new Dictionary<string, object>
					{
						{ "unique_account_identifier",  PrincipalInfo.CurrentPrincipal.GetContactId() },
						{ "account_registration_date", customerContact.Created },
						{ "account_last_modified", customerContact.Modified }
					}
				};

			var emd = new Dictionary<string, object>
				{
					{ "customer_account_info", customerAccountInfos}
				};

			session.Attachment = new Attachment
			{
				ContentType = "application/vnd.klarna.internal.emd-v2+json",
				Body = JsonConvert.SerializeObject(emd, converter)
			};
		}
        return session;
    }
}
```

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

Read more about the different parameters: https://developers.klarna.com/api/#payments-api-create-a-new-credit-session.

When `UseAttachments` is set to true, extra information can be send to Klarna. The code snippet above (DemoSessionBuilder) shows an example how you can implement this. [Here's the full documentation](https://developers.klarna.com/documentation/klarna-payments/integration-guide/create-session/#extra-merchant-data) by Klarna.

</details>

<details>
  <summary>Authorize payment client-side (click to expand)</summary>

The last step just before creating an order is to do an [authorization call](https://developers.klarna.com/documentation/klarna-payments/integration-guide/authorize/). In this call we will provide Klarna with any missing personal information (which might be missing due to legislation). Up until now, no personal information might have been synced to Klarna, which makes risk assessment quite hard to accomplish. During the authorize call we provide Klarna with the required personal information (billing-/shipping address, customer info). Klarna will conduct a full risk assessment after which it will provide immediate feedback, which is described on the previously linked [docs](https://developers.klarna.com/documentation/klarna-payments/integration-guide/authorize/).

As Foundation supports both authenticated and anonymous checkout, we have multiple ways to retrieve personal information for the current customer.

Ways to retrieve personal information (PI):

- Authenticated user
  - In this case we expect that (most of) the personal information exists server side. We do an api call to the provided KlarnaPaymentController (url: "/klarnaapi/personal") to retrieve personal information. Due to the way the Foundation checkout process is set up, we have to provide the currently selected billing address id; because it is not stored server side (yet).
- Anonymous user
  - In this case we expect that no information exists server side. We retrieve personal information from form fields and use that to populate the object with personal information.

If anything goes wrong it could be that the Klarna widget will display a pop-up, allowing the user to recover from any errors. In case of non-recoverable error(s); the widget should be hidden and we should inform the user to select a different payment method. If the authorization is not approved, also check the show_form value returned, if it is false, then do not show the Klarna payment method anymore to the user in that user session. The happy flow (no errors) would mean that we will retrieve an authorization token from Klarna and can continue with the checkout process.
Receiving an authorization token means that the risk assessment succeeded and we're able to complete the order. The authorization token is provided during the form post to Optimizely (purchase). This authorization token is important because it allows us to make sure no changes were made client side (as you can change the cart items in the authorization call as well).

**Checkout flow:**

Step 1: Showing checkout page (by default no personal information is shared)
- Server side - During checkout we use the CreateOrUpdateSession to update the session at Klarna (this does not contain any PI)

Step 2: Placing order (personal information is shared)
- Client side - When the user clicks on 'Place order' we use the Klarna javascript library to do an authorize call, providing the necessary customer information. Best practice is to disable the place order button after the user clicks it to prevent subsequent calls.
  - If authorize succeeds we receive an authorization token, which we add to the checkout form and pass on to our server
  - If authorize fails, for example if there are no offers based on the user's personal info, we flip a boolean on the user's cart server side. That boolean will allow the CreateOrUpdateSession to send PI to Klarna in any subsequent call (IKlarnaPaymentsService - AllowedToSharePersonalInformation).
- Server side - After authorize we take our cart and using this session and the authorization token we can create an order in Klarna.
  - If creating an order fails, the authorize request has been tampered with and the payment fails

In your own implementation you can use Klarna.Payments.js as a reference implementation. The existing Checkout.js has been modified slightly in order to 1. (re-)load the Klarna widget after updating the order summary and 2. do an authorization call to Optimizely on `jsCheckoutForm` submit.

### Finalizing
The user may, in some cases, need to introduce data a second time (e.g. providing a legal authorization, or selecting a bank account). We call this the finalize step.

By default, the SDK performs authorization and finalization automatically after each other, but you can request to perform these separately if the flow in your app requires it. If that’s the case, your listener will be notified with a `finalizeRequired` parameter set to `true`.

If the session needs to be finalized, you’ll need to perform this last step to get an authorization token. The finalization should be done just before the purchase is completed, meaning the last step in a multi-step checkout.

You can finalize the session by calling the view’s `finalize()` method.

For more information please see: [Klarna Payments Finalize the authorization](https://docs.klarna.com/klarna-payments/api-call-descriptions/authorize-the-purchase/#finalize-the-authorization).
</details>

<details>
  <summary>Create order (click to expand)</summary>

The KlarnaPaymentGateway will create an order at Klarna when the authorization (client-side) is done. The ISessionBuilder is called again to override the default values or set other extra values when necessary. When the Gateway returns true (indicating the payment is processed) a PurchaseOrder can be created. This should be done by the developer, the Foundation demo site contains an example implementation.

</details>

<details>
  <summary>Order notes (click to expand)</summary>
  
The KlarnaPaymentGateway save notes about payment updates to the order.

![Order notes](/docs/screenshots/order-notes.PNG?raw=true "Order notes")

</details>

<details>
  <summary>Foundation demo site implementation (click to expand)</summary>
  
This repository includes the [Foundation demo site](https://github.com/Geta/Klarna/tree/master/demo) which contains an example implementation of this package. The implementation requires both frontend and backend changes.

**Load Klarna JS script**

Load the Klarna API Javascript.

```
<script src="https://x.klarnacdn.net/kp/lib/v1/api.js" async></script>
```

**Frontend implementation**

There are a few frontend changes that are required.

- Load and initialize (define settings) the Klarna Payments widget
- Authorize payment when visitor clicks the purchase button. The authorize action can be used to send some additional personal. Some countries (EU) we can only send personal information in the last (authorize) step. See more info about the [authorize step here](https://docs.klarna.com/klarna-payments/api-call-descriptions/authorize-the-purchase/)

Example implementation: [Klarna.Payments.js](/demo/Foundation/src/Foundation/wwwroot/js/common/Klarna.Payments.js) and [checkout.js](/demo/Foundation/src/Foundation/Features/Checkout/checkout.js) (search for KlarnaPayments)

**API controller - frontend and callback communication**

The [KlarnaPaymentsApiController](/demo/Foundation/src/Foundation/Features/Api/KlarnaPaymentsApiController.cs) contains actions that are used by the frontend and for Klarna callbacks (confirmation, fraud, notification, and push).

- GetpersonalInformation - Get personal information for the authorization call. See the section 'Call authorize client-side' for more explanation.
- AllowSharingOfPersonalInformation - Check if the personal information can be shared. See the section 'Call authorize client-side' for more explanation.

**Load and display payment - Foundation**

- [\_KlarnaPaymentsPaymentMethod.cshtml](/demo/Foundation/src/Foundation/Features/Checkout/_KlarnaPaymentsPaymentMethod.cshtml) - display Klarna Payment method
- [\_KlarnaPaymentsConfirmation.cshtml](/demo/Foundation/src/Foundation/Features/MyAccount/OrderConfirmation/_KlarnaPaymentsConfirmation.cshtml) - Klarna Payments confirmation view
- [KlarnaPaymentsPaymentOption.cs](/demo/Foundation/src/Foundation/Features/Checkout/Payments/KlarnaPaymentsPaymentOption.cs)
  - See PostProcess - Set the payment status to pending when the fraud status is pending
- Implement AuthorizationToken on the [CheckoutViewModel](/demo/Foundation/src/Foundation/Features/Checkout/ViewModels/CheckoutViewModel.cs), add HiddenField on [Checkout.cshtml](/demo/Foundation/src/Foundation/Features/Checkout/Checkout.cshtml)

**Process payment - Foundation**

- [CheckoutService](/demo/Foundation/src/Foundation/Features/Checkout/Services/CheckoutService.cs) `CreateAndAddPaymentToCart` - Set authorization token on payment object. This should be done before calling the payment gateway - `cart.ProcessPayments(_paymentProcessor, _orderGroupCalculator)`
- Call `CreateOrUpdateSession` with the updated Cart when you make changes to the cart (add coupon codes, change shipping address, line item changes etc) [CheckoutController](/demo/Foundation/src/Foundation/Features/Checkout/CheckoutController.cs)
- Call the `Complete` method to redirect the visitor to the confirmation page after creating a PurchaseOrder

Note: if you're not using serialized carts you need to set the OrderNumberMethod property on the cart like below code snippet. This package contains an implementation of the IOrderNumberGenerator. During payment authorization (so before a purchase order is created) it's mandatory to send the order number to Klarna. The custom implementation in the package generates an order number and saves it on the cart. When the SaveAsPurchaseOrder method is called the implementation will return the generated order number from the cart.

```
if (cart is Mediachase.Commerce.Orders.Cart) // old (not serialized) carts don't use the IOrderNumberGenerator
{
    var orderNumberGenerator = ServiceLocator.Current.GetInstance<IOrderNumberGenerator>();
    ((Mediachase.Commerce.Orders.Cart)cart).OrderNumberMethod = orderNumberGenerator.GenerateOrderNumber;
}
```

</details>

## Package maintainer

https://github.com/frederikvig

## Changelog

[Changelog](../../CHANGELOG.md)
