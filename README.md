EPiServer Klarna payments integration
=============

## What is Klarna.Payments?

Klarna.Payments is a library which helps to integrate Klarna Payments as one of the payment options in your EPiServer Commerce sites.
This library consists of two assemblies: 
* Klarna.Payments basically the integration between EPiServer and the Klarna Payments API (v3) (https://developers.klarna.com/api/#payments-api)
* Klarna.Payments.CommerceManager contains a usercontrol for the payment method configuration in Commerce Manager

### Payment process
- **Klarna: Create session** - A session is created at Klarna
- **Klarna: Authorization**  - The authorization step is done client-side 
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

![Payment method settings](/Netaxept/docs/screenshots/overview.PNG?raw=true "Payment method settings")

**Connection string**
Connection string configurations for the connection with the Klarna APi

**Widget settings**
Set the colors and border size for the Klarna widget. The Klarna logo should be placed by the developer somewhere on the checkout/payment page.

**Other settings**
Confirmation url is called when calling this method:
```
_klarnaService.RedirectToConfirmationUrl(purchaseOrder);
```
Notification url is called by Klarna for fraud updates. See further in the documentation for an example implementation. The 'Send product and image URL' checkbox indicates if the product (in cart) page and image URL should be send to Klarna. When the 'Use attachment' checkbox is checked the developer should send extra information to Klarna. See the Klarna documentation for more explanation: https://developers.klarna.com/en/se/kco-v2/checkout/use-cases. The 'PreAssement' checkbox indicates if customer information should be send to Klarna to check if the customer is able to buy via Klarna.

```
sessionRequest.Customer = new Customer
{
    DateOfBirth = "1980-01-01",
    Gender = "Male",
    LastFourSsn = "1234"
};
```

![Payment method settings](/Netaxept/docs/screenshots/parameters.PNG?raw=true "Payment method parameters")

**Note: If the parameters tab is empty (or gateway class is missing), make sure you have installed the commerce manager nuget (see above)**

In the **Markets** tab select a market for which this payment will be available.
