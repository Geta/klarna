EPiServer Klarna payments integration
=============

## What is Klarna.Payments?

Klarna.Payments is a library which helps to integrate Klarna Payments as one of the payment options in your EPiServer Commerce sites.
This library consists of two assemblies: 
* Klarna.Payments basically the integration between EPiServer and the Klarna Payments API (v3) (https://developers.klarna.com/api/#payments-api)
* Klarna.Payments.CommerceManager contains a usercontrol for the payment method configuration in Commerce Manager

### Payment process
- **Klarna: Create session** - A session is created at Klarna
- **Klarna: Authorization**  - The authorization step is done client-side (when the visitor presses the Purchase button)
- **Klarna: Create order** - Create order at Klarna
- **EPiServer: Create purchase order** - Create purchase order in EPiServer

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
Confirmation url is called when calling this method:
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
        if (includePersonalInformation && configuration.IsCustomerPreAssessmentEnabled)
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

The following properties are set default:
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

When the 'Use attachment' checkbox is checked extra information can be send to Klarna. The code snippet above shows an example how you can implement this. Full documentation regarding this topic can be found here: https://developers.klarna.com/en/se/kco-v2/checkout/use-cases

### Call authorize client-side
// TODO: Brian

### Create order
The KlarnaPaymentGateway will create an order at Klarna when the authorization (client-side) is done. The SessionBuilder class is called again to override the default values or set other extra values when necessary. When the Gateway returns true (indicating the payment is processed) a PurchaseOrder can be created. This should be done by the developer, you can have a look at the QuickSilver demo site for an example implementation.

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

