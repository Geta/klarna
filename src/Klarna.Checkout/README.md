EPiServer Klarna checkout integration
=============

## What is Klarna.Checkout?

Klarna.Checkout is a library which helps to integrate Klarna Checkout as one of the payment options in your EPiServer Commerce sites.
This library consists of two assemblies. Both are mandatory for a creating an integration between EPiServer and Klarna: 
* Klarna.Checkout is the integration between EPiServer and the Klarna Checkout API (https://developers.klarna.com/api/#checkout-api-order)
* Klarna.Checkout.CommerceManager contains a usercontrol for the payment method configuration in Commerce Manager

### Payment process
- **Visitor visit's checkout page** - Klarna order is created or updated 
- **Klarna checkout widget is loaded (payment option)**  
    - Visitor can enter shipping and payment information
    - Callback url's are called for updating information in EPiServer, but also to return information like available shipment option and calculate taxes.
- **Visitor presses the Place order**
    - Order validation callback is called for the last checks before finalizing the order
- **Finalize order at Klarna**
- **Visitor is redirected to confirmation callback**
    - Purchase order is created in EPiServer
- **Visitor is redirected to confirmation page
- **Klarna - fraud status notification** - When the Klarna order is pending then fraud status notification are send to the configured notification URL (configured in Commerce Manager)

More information about the Klarna Checkout flow: https://developers.klarna.com/en/gb/kco-v3/checkout

## How to get started?

Start by installing NuGet packages (use [NuGet](http://nuget.episerver.com/)):

    Install-Package Klarna.Checkout

For the Commerce Manager site run the following package:

    Install-Package Klarna.Checkout.CommerceManager

## Setup

### Configure Commerce Manager

Login into Commerce Manager and open **Administration -> Order System -> Payments**. Then click **New** and in **Overview** tab fill:

- **Name(*)**
- **System Keyword(*)** - KlarnaCheckout (the integration will not work when something else is entered in this field)
- **Language(*)** - allows a specific language to be specified for the payment gateway
- **Class Name(*)** - choose **Klarna.Checkout.KlarnaCheckoutGateway**
- **Payment Class(*)** - choose **Mediachase.Commerce.Orders.OtherPayment**
- **IsActive** - **Yes**

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

### Quicksilver demo site implementation




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

