EPiServer Klarna payments integration
=============

## What is Klarna.Payments?

Klarna.Payments is a library which helps to integrate Klarna Payments as one of the payment options in your EPiServer Commerce sites.
This library consists of two assemblies: 
* Klarna.Payments basically the integration between EPiServer and the Klarna Payments API (v3) (https://developers.klarna.com/api/#payments-api)
* Klarna.Payments.CommerceManager contains a usercontrol for the payment method configuration in Commerce Manager

### Payment process
- **Visitor visit's checkout page** - Klarna session is created or updated 
- **Klarna widget is loaded (payment option)**  
- **Visitor presses the Purchase button**  - Klarna payment authorize is called client-side 
- **Klarna: Create order** - When payment authorization is completed then create order (payment gateway) at Klarna
- **EPiServer: Create purchase order** - Create purchase order in EPiServer
- **Klarna - fraud status notification** - When the Klarna order is pending then fraud status notification are send to the configured notification URL (configured in Commerce Manager)

More information about the Klarna Payments flow: https://developers.klarna.com/en/gb/kco-v3/klarna-payment-methods

## How to get started?

Start by installing NuGet packages (use [NuGet](http://nuget.episerver.com/)):

    Install-Package Klarna.Payments

For the Commerce Manager site run the following package:

    Install-Package Klarna.Payments.CommerceManager

## Setup

### Configure Commerce Manager

Login into Commerce Manager and open **Administration -> Order System -> Payments**. Then click **New** and in **Overview** tab fill:

- **Name**
- **System Keyword** - KlarnaPayments
- **Language**
- **Class Name** - choose **Klarna.Payments.KlarnaPaymentGateway**
- **Payment Class** - choose **Mediachase.Commerce.Orders.OtherPayment**
- **IsActive** - **Yes**
- select shipping methods available for this payment
- navigate to parameters tab and fill in settings (see screenshot below)

![Payment method settings](/docs/screenshots/payment-overview.PNG?raw=true "Payment method settings")

**Connection string**

Connection string configurations for the connection with the Klarna APi.

**Widget settings**

Set the colors and border size for the Klarna widget. The Klarna logo should be placed by the developer somewhere on the checkout/payment page.

**Other settings**

After payment is completed the confirmation url must be called. This can be done with this method:
```
_klarnaService.RedirectToConfirmationUrl(purchaseOrder);
```
Notification url is called by Klarna for fraud updates. See further in the documentation for an example implementation. The 'Send product and image URL' checkbox indicates if the product (in cart) page and image URL should be send to Klarna. When the 'Use attachment' checkbox is checked the developer should send extra information to Klarna. See the Klarna documentation for more explanation: https://developers.klarna.com/en/se/kco-v2/checkout/use-cases.

The 'PreAssement' field indicates if customer information should be send to Klarna. Klarna will review this information to verify if the customer can buy via Klarna. This setting can be enabled per country. Below a code snippet for sending customer information. An implementation of the SessionBuilder class can be used for setting this information. The SessionBuilder is explained later in this document.

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

In the **Markets** tab select a market for which this payment will be available.

### Creating session
A session at Klarna should be created when the visitor is on the checkout page. The CreateOrUpdateSession method will create a new session when it does not exists or update the current one.

```
await _klarnaService.CreateOrUpdateSession(Cart);
```

It's mandatory to create an implementation of the SessionBuilder. The Build method is called after all default values are set. This way the developer is able to override values or set missing values. The includePersonalInformation parameter indicates if personal information can be send to Klarna. There are some restrictions for certain countries. For example, countries in the EU can only send personal information on the last step of the payment process. Below an example implementation of the SessionBuilder class.

```
public class DemoSessionBuilder : SessionBuilder
{
    public override Session Build(Session session, ICart cart, Klarna.Payments.Configuration configuration, bool includePersonalInformation = false)
    {
        if (includePersonalInformation && configuration.CustomerPreAssessmentCountries.Any(c => cart.Market.Countries.Contains(c)))
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

### Call authorize client-side
The last step just before creating an order is to do an [authorization call](https://developers.klarna.com/en/gb/kco-v3/klarna-payment-methods/3-authorize). In this call we will provide Klarna with any missing personal information (which might be missing due to legislation). Up until now no personal information might have been synced to Klarna, which makes risk assessment quite hard to accomplish. During the authorize call we provide Klarna with the required personal information (billing-/shipping address, customer info). Klarna will conduct a full risk assessment after which it will provide immediate feedback, which is described on the previously linked [docs](https://developers.klarna.com/en/gb/kco-v3/klarna-payment-methods/3-authorize).
As Quicksilver supports both authenticated and anonymous checkout, we have multiple ways to retrieve personal information for the current customer.

Ways to retrieve personal information:
- Authenticated user
    - In this case we expect that (most of) the personal information exists server side. We do an api call to the provided KlarnaPaymentController (url: "/klarnaapi/personal") to retrieve personal information. Due to the way the Quicksilver checkout process is set up, we have to provide the currently selected billing address id; because it is not stored server side (yet). 
- Anonymous user
    - In this case we expect that no information exists server side. We retrieve personal information from form fields and use that to populate the object with personal information. 

If anything goes wrong it could be that the Klarna widget will display a pop-up, allowing the user to recover from any errors. In case of non-recoverable error(s); the widget should be hidden and we should inform the user to select a different payment method. The happy flow (no errors) would mean that we will retrieve an authorization token from Klarna and can continue with the checkout process.
Receiving an authorization token means that the risk assessment succeeded and we're able to complete the order. The authorization token is provided during the form post to Epi (purchase). This authorization token is important because it allows us to make sure no changes were made client side (as you can change the cart items in the autorization call as well).

Checkout flow:
- Server side - During checkout we use the CreateOrUpdateSession to update the session at Klarna
- Client side - When the user presses purchase we update the session with personal information
- Server side - After authorize we take our cart and create a session based on the information we have (which is our 'thruth'), using this session and the authorization token we can create an order in Klarna
    - If creating an order fails, the authorize request has been tampered with and the payment fails
    
 In your own implementation you can use Checkout.Klarna.js as a reference implementation. The existing Checkout.js has been modified slightly in order to 1. (re-)load the clarna widget after updating the order summary and 2. do an authorization call to epi on 'jsCheckoutForm' submit.

### Create order
The KlarnaPaymentGateway will create an order at Klarna when the authorization (client-side) is done. The SessionBuilder class is called again to override the default values or set other extra values when necessary. When the Gateway returns true (indicating the payment is processed) a PurchaseOrder can be created. This should be done by the developer, the QuickSilver demo site contains an example implementation.

### Fraud status notifications
In Commerce Manager the notification URL can be configured. Klarna will call this URL for notifications for an orders that needs an additional review (fraud reasons). The IKlarnaService includes a method for handling fraud notifications. Below an example implementation.

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

        _klarnaService.FraudUpdate(notification);
    }
    return Ok();
}
```

### Order notes
The KlarnaPaymentGateway save notes about payment updates at the order.
![Order notes](/docs/screenshots/order-notes.PNG?raw=true "Order notes")

