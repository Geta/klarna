EPiServer Klarna Payments integration
=============

## Description
Klarna.Payments is a library which helps to integrate Klarna Payments as one of the payment options in your EPiServer Commerce sites.
This library consists of two assemblies. Both are mandatory for a creating an integration between EPiServer and Klarna. 
Klarna.Payments is the integration between EPiServer and the Klarna Payments API (https://developers.klarna.com/api/#payments-api).
Klarna.Payments.CommerceManager contains a usercontrol for the payment method configuration in Commerce Manager.

## Integration

![Klarna Payments integration](https://github.com/Geta/Klarna/raw/master/docs/images/klarna-payments-integration.png)

## Features
* Loading Klarna Payments widget
* Cancel payments
* Returns
* Multi shipments
* Fraud checks + notifications
* Payment step history saved on order notes
* Configurations in Commerce Manager

### Payment process
- **Customer visits checkout page** - Klarna session is created or updated 
- **Klarna widget is loaded (payment option)**  
- **Customer presses the Purchase button**  - Klarna payment authorize is called client-side 
- **Klarna: Create order** - When payment authorization is completed then create order (payment gateway) at Klarna
- **EPiServer: Create purchase order** - Create purchase order in EPiServer
- **Klarna - fraud status notification** - When the Klarna order is pending then fraud status notification are send to the configured notification URL (configured in Commerce Manager)

More information about the Klarna Payments flow: https://developers.klarna.com/en/gb/kco-v3/klarna-payment-methods

<details>
  <summary>Setup (click to expand)</summary>

Start by installing NuGet packages (use [NuGet](http://nuget.episerver.com/)):

    Install-Package Klarna.Payments.v3

For the Commerce Manager site run the following package:

    Install-Package Klarna.Payments.CommerceManager.v3
</details>
<details>
  <summary>Configure Commerce Manager (click to expand)</summary>

Login into Commerce Manager and open **Administration -> Order System -> Payments**. Then click **New** and in **Overview** tab fill:

- **Name(*)**
- **System Keyword(*)** - KlarnaPayments (the integration will not work when something else is entered in this field)
- **Language(*)** - allows a specific language to be specified for the payment gateway
- **Class Name(*)** - choose **Klarna.Payments.KlarnaPaymentGateway**
- **Payment Class(*)** - choose **Mediachase.Commerce.Orders.OtherPayment**
- **IsActive** - **Yes**
- **Supports Recurring** - **No** - this Klarna Payments integration does not support recurring payments

(*) mandatory
- select shipping methods available for this payment

![Payment method settings](/docs/screenshots/payment-overview.PNG?raw=true "Payment method settings")

- navigate to parameters tab and fill out the Klarna configurations. Configurations are market specific so first select a market. (see screenshot below)

**Connection string**

Connection string configurations for the connection with the Klarna APi. See the Klarna documentation for the API endpoints: https://developers.klarna.com/api/#api-urls. Klarna API requires HTTPS.

**Widget settings**

Set the colors and border size for the Klarna widget. The Klarna logo should be placed by the developer somewhere on the checkout/payment page.

**Other settings**

After payment is completed the confirmation url must be called. This can be done with this method:
```csharp
var result = _klarnaPaymentsService.Complete(purchaseOrder);
if (result.IsRedirect)
{
    return Redirect(result.RedirectUrl);
}
```
Notification url is called by Klarna for fraud updates. See further in the documentation for an example implementation. The 'Send product and image URL' checkbox indicates if the product (in cart) page and image URL should be sent to Klarna. When the 'Use attachment' checkbox is checked the developer should send extra information to Klarna. See the Klarna documentation for more explanation: https://developers.klarna.com/en/se/kco-v2/checkout/use-cases.

The 'Pre-assesment' field indicates if customer information should be sent to Klarna prior to authorization. Klarna will review this information to verify if the customer can buy via Klarna. This option is only available in the U.S. market and will be ignored for all other markets. Below a code snippet for sending customer information. An implementation of the ISessionBuilder can be used for setting this information. The ISessionBuilder is explained later in this document.

```
sessionRequest.Customer = new Customer
{
    DateOfBirth = "1980-01-01",
    Gender = "Male",
    LastFourSsn = "1234"
};
```

![Payment method settings](/docs/screenshots/payment-parameters.PNG?raw=true "Payment method parameters")

**Note: If the parameters tab is empty (or gateway class is missing), make sure you have installed the commerce manager nuget (see above)**

- In the **Markets** tab select a market for which this payment will be available.
</details>

<details>
  <summary>Creating session (click to expand)</summary>

A session at Klarna should be created when the visitor is on the checkout page. The CreateOrUpdateSession method will create a new session when it does not exists or update the current one. This method also accepts an optional parameter of the type IDictionary<string, object>. This parameter can be used to pass extra information that can be used in the session builder.

```
await _klarnaPaymentsService.CreateOrUpdateSession(Cart);
```

It's possible to create an implementation of the ISessionBuilder. The Build method is called after all default values are set. This way the developer is able to override values or set missing values. The includePersonalInformation parameter indicates if personal information can be sent to Klarna. There are some restrictions for certain countries. For example, countries in the EU can only send personal information on the last step of the payment process. Below an example implementation of a DemoSessionBuilder.

```
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

        if (configuration.UseAttachments)
        {
            var converter = new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"
            };

            var customerAccountInfos = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "unique_account_identifier",  "Test Testperson" },
                    { "account_registration_date", DateTime.Now },
                    { "account_last_modified", DateTime.Now }
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

Read more about the different parameters: https://developers.klarna.com/api/#payments-api-create-a-new-session.

When the 'Use attachment' checkbox is checked extra information can be send to Klarna. The code snippet above (DemoSessionBuilder) shows an example how you can implement this. Full documentation about this topic can be found here: https://developers.klarna.com/en/se/kco-v2/checkout/use-cases

</details>

<details>
  <summary>Authorize payment client-side (click to expand)</summary>

The last step just before creating an order is to do an [authorization call](https://developers.klarna.com/en/gb/kco-v3/klarna-payment-methods/3-authorize). In this call we will provide Klarna with any missing personal information (which might be missing due to legislation). Up until now no personal information might have been synced to Klarna, which makes risk assessment quite hard to accomplish. During the authorize call we provide Klarna with the required personal information (billing-/shipping address, customer info). Klarna will conduct a full risk assessment after which it will provide immediate feedback, which is described on the previously linked [docs](https://developers.klarna.com/en/gb/kco-v3/klarna-payment-methods/3-authorize).
As Quicksilver supports both authenticated and anonymous checkout, we have multiple ways to retrieve personal information for the current customer.

Ways to retrieve personal information (PI):
- Authenticated user
    - In this case we expect that (most of) the personal information exists server side. We do an api call to the provided KlarnaPaymentController (url: "/klarnaapi/personal") to retrieve personal information. Due to the way the Quicksilver checkout process is set up, we have to provide the currently selected billing address id; because it is not stored server side (yet). 
- Anonymous user
    - In this case we expect that no information exists server side. We retrieve personal information from form fields and use that to populate the object with personal information. 

If anything goes wrong it could be that the Klarna widget will display a pop-up, allowing the user to recover from any errors. In case of non-recoverable error(s); the widget should be hidden and we should inform the user to select a different payment method. The happy flow (no errors) would mean that we will retrieve an authorization token from Klarna and can continue with the checkout process.
Receiving an authorization token means that the risk assessment succeeded and we're able to complete the order. The authorization token is provided during the form post to Epi (purchase). This authorization token is important because it allows us to make sure no changes were made client side (as you can change the cart items in the authorization call as well).

Checkout flow:
- Server side - During checkout we use the CreateOrUpdateSession to update the session at Klarna (this does not contain any PI)
- Client side - When the user clicks on 'Place order' we use the Klarna javascript library to do an authorize call, providing the necessary PI.
    - If authorize succeeds we receive an authorization token, which we add to the checkout form and pass on to our server
    - If authorize fails, for example if there are no offers based on the user's personal info, we flip a boolean on the user's cart server side. That boolean will allow the CreateOrUpdateSession to send PI to Klarna in any subsequent call (IKlarnaPaymentsService - AllowedToSharePersonalInformation). 
- Server side - After authorize we take our cart and create another 'clean' session based on the information we have (which is our 'truth'), using this session and the authorization token we can create an order in Klarna.
    - If creating an order fails, the authorize request has been tampered with and the payment fails
    
 In your own implementation you can use Checkout.Klarna.js as a reference implementation. The existing Checkout.js has been modified slightly in order to 1. (re-)load the Klarna widget after updating the order summary and 2. do an authorization call to epi on 'jsCheckoutForm' submit.
</details>

<details>
  <summary>Create order (click to expand)</summary>

The KlarnaPaymentGateway will create an order at Klarna when the authorization (client-side) is done. The ISessionBuilder is called again to override the default values or set other extra values when necessary. When the Gateway returns true (indicating the payment is processed) a PurchaseOrder can be created. This should be done by the developer, the QuickSilver demo site contains an example implementation. 
</details>

<details>
  <summary>Fraud status notifications (click to expand)</summary>
  
In Commerce Manager the notification URL can be configured. Klarna will call this URL for notifications for an orders that needs an additional review (fraud reasons). The IKlarnaPaymentsService includes a method for handling fraud notifications. Below an example implementation.

```
[Route("fraud/")]
[AcceptVerbs("Post")]
[HttpPost]
public IHttpActionResult FraudNotification()
{
    var requestParams = Request.Content.ReadAsStringAsync().Result;

    _log.Error("KlarnaPaymentController.FraudNotification called: " + requestParams);

    if (!string.IsNullOrEmpty(requestParams))
    {
        var notification = JsonConvert.DeserializeObject<NotificationModel>(requestParams);

        _klarnaPaymentsService.FraudUpdate(notification);
    }
    return Ok();
}

When a payment needs an additional review, the payment in EPiServer is set to the status PENDING and the order to ONHOLD. When the fraud status callback URL is called and the payment is accepted the payment status will be set to PROCESSED and the order to ONHOLD. If the payment is rejected by Klarna the payment status is set to FAILED.
```
</details>

<details>
  <summary>Order notes (click to expand)</summary>
  
The KlarnaPaymentGateway save notes about payment updates at the order.

![Order notes](/docs/screenshots/order-notes.PNG?raw=true "Order notes")
</details>

<details>
  <summary>Quicksilver demo site implementation (click to expand)</summary>
  
This repository includes the Quicksilver demo site (https://github.com/Geta/Klarna/tree/master/demo) which contains an example implementation of this package. The following steps are done for implementing this package.
- Load Klarna api.js on [Layout.cshtml](/demo/Sources/EPiServer.Reference.Commerce.Site/Views/Shared/_Layout.cshtml#L87)
- Implement [Checkout.Klarna.js](/demo/Sources/EPiServer.Reference.Commerce.Site/Scripts/js/Checkout.Klarna.js) 
- [Reload the Klarna widget](/demo/Sources/EPiServer.Reference.Commerce.Site/Scripts/js/Checkout.js#L167) each time something changed on the checkout page
- [Execute authorization](/demo/Sources/EPiServer.Reference.Commerce.Site/Scripts/js/Checkout.js#L14) at Klarna when Purchase button is clicked
- Implement [API controller](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Checkout/Controllers/KlarnaPaymentController.cs)
    - [Get personal information](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Checkout/Controllers/KlarnaPaymentController.cs#L39) for the authorization call. See the section 'Call authorize client-side' for more explanation.
    - Check [if the personal information can be shared](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Checkout/Controllers/KlarnaPaymentController.cs#L55). See the section 'Call authorize client-side' for more explanation.
    - Endpoint for [fraud notifications](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Checkout/Controllers/KlarnaPaymentController.cs#L69) pushed by Klarna. This URL can be configured in Commerce Manager, see the 'Configure Commerce Manager' section.
- Add [KlarnaPaymentsPaymentMethod.cshtml](/demo/Sources/EPiServer.Reference.Commerce.Site/Views/Shared/_KlarnaPaymentsPaymentMethod.cshtml) view
- Add [KlarnaPaymentMethodsConfirmation.cshtml](/demo/Sources/EPiServer.Reference.Commerce.Site/Views/Shared/_KlarnaPaymentsConfirmation.cshtml) view
- Create [KlarnaPaymentsPaymentMethod](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Payment/PaymentMethods/KlarnaPaymentsPaymentMethod.cs)
    - [Set the payment status to pending](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Payment/PaymentMethods/KlarnaPaymentsPaymentMethod.cs#L41) when the fraud status is pending     
- Create [KlarnaPaymentsViewModel](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Payment/ViewModels/KlarnaPaymentsViewModel.cs)
- [Return KlarnaViewModel](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Payment/ViewModels/PaymentMethodViewModelResolver.cs) in PaymentMethodViewModelResolver
- [Define Authorization token property](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Checkout/ViewModels/CheckoutViewModel.cs#L73) on view model, add hiddenfield on [Single-](/demo/Sources/EPiServer.Reference.Commerce.Site/Views/Checkout/SingleShipmentCheckout.cshtml#L87) and [MultiShipmentCheckout.cshtml](/demo/Sources/EPiServer.Reference.Commerce.Site/Views/Checkout/MultiShipmentCheckout.cshtml#L67)
- [Set authorization token](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Checkout/Services/CheckoutService.cs#L109) on payment object. This should be done before calling the payment gateway - cart.ProcessPayments(_paymentProcessor, _orderGroupCalculator);
- Call CreateOrUpdateSession method in the [Index](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Checkout/Controllers/CheckoutController.cs#L84), [Update](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Checkout/Controllers/CheckoutController.cs#L116) and [ChangeAddress](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Checkout/Controllers/CheckoutController.cs#L123) action of the CheckoutController
- Call the CompleteAndRedirect to [redirect](/demo/Sources/EPiServer.Reference.Commerce.Site/Features/Checkout/Controllers/CheckoutController.cs#L221) the visitor to the confirmation page after creating a PurchaseOrder

Note: if you're not using serialized carts you need to set the OrderNumberMethod property on the cart like below code snippet. This package contains an implementation of the IOrderNumberGenerator. During payment authorization (so before a purchase order is created) it's mandatory to send the order number to Klarna. The custom implementation in the package generates an order number and saves it on the cart. When the SaveAsPurchaseOrder method is called the implementation will return the generated order number from the cart. 

```
if (cart is Mediachase.Commerce.Orders.Cart) // old (not serialized) carts don't use the IOrderNumberGenerator
{
    var orderNumberGenerator = ServiceLocator.Current.GetInstance<IOrderNumberGenerator>();
    ((Mediachase.Commerce.Orders.Cart)cart).OrderNumberMethod = orderNumberGenerator.GenerateOrderNumber;
}
```
</details>

## Demo
https://kp-klarna.geta.no

## Package maintainer
https://github.com/patkleef

## Changelog
[Changelog](../../CHANGELOG.md)
