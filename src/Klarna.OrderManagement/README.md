Optimizely Klarna Order Management
=============

## Description

Klarna.OrderManagement is a library for processing a Klarna payments in Optimizely Commerce. This package supports both payments of Klarna Payments and Klarna Checkout. 

More about Klarna Order Management: https://docs.klarna.com/order-management/.

## Features
* Capture
* Refund
* Release remaining authorization
* Cancel
* Support multi shipment payments

### Steps integrated with Optimizely Commerce
- **Capture** - either partially (multi-shipment) or full capture the payment amount
- **Refund** - either partially or full refund an amount
- **Release remaining authorization** - release remaining authorization when the payment amount has not been fully captured. Note: this action should only occur when for example order line 1 is captured and the remaining order lines will not be captured.
- **Cancel** - full cancel of an order

### (*) Other steps only available in the code
- **Get Klarna order**
- **Update merchant reference** - update merchant reference 1 and 2
- **Trigger send out**
- **Extend authorization time**
- **Update customer information**

(*) Not integrated in the Optimizely Commerce order process

<details>
  <summary>Setup (click to expand)</summary>

Start by installing NuGet packages (use [NuGet](http://nuget.optimizely.com/)):

```
dotnet add package Klarna.OrderManagement.v3
```

Both Klarna Payments and Klarna Checkout have dependencies on the Klarna Order Management package. It's more likely that one of those packages are installed, which automatically install this package.    
</details>

<details>
  <summary>Capture (click to expand)</summary>

Capturing payments is done by completing a shipment in Optimizely Order Management. Follow these steps to complete a shipment:

- Open the order in the Order Management screen
- Go to Order details and Form details for the order
- Under shipments and the line item click 'Release'
- This will open a modal window, click 'Release shipment'
- Then click 'Add to PickList'
- And 'Complate shipment'
- Complete shipment for corresponding order
    - You can enter tracking number in the 'Complete Shipment' pop-up, this will be available as shipping information in Klarna
    - The 'OK' button triggers the payment gateway to do a capture, if capturing the payment succeeds the pop up will close. Otherwise you'll receive an error message in the pop up or, if there's something wrong with the payment there should be a new order note with exception information on the order.

Look at the [order notes section](#order-notes) for example order notes regarding captures.

####  Partial capture
Upon completing a shipment in a multi-shipment scenario, a partial capture will be done towards Klarna. The partial capture will capture the amount that has to be captured for that specific shipment. If all shipments are completed, the full order amount will have been captured.


#### Change Capture data
By default all capture data should be set automatically. However, similar to Klarna Payment sessions, it is possible to change capture data before it is sent to Klarna. In order to do so you can create an implementation of ``ICaptureBuilder`` and register it with your IoC container.
```csharp
public class DemoCaptureBuilder : ICaptureBuilder
{
    public CaptureData Build(CaptureData captureData, IOrderGroup orderGroup, IOrderForm returnOrderForm, IPayment payment)
    {
        // Here you can make changes to captureData if needed
        return captureData;
    }
}
```
</details>
<details>
  <summary>Release remaining authorization (click to expand)</summary>

In a multi-shipment scenario, each individual shipment can be completed or cancelled. For instance, an order with two shipments, one shipments was fullfilled and the other one was cancelled (partially completed). This means the remaining authorized amount at Klarna needs to be released.

When the last shipment is handled, the payment gateway is called to release the remaining authorized amount at Klarna. The transactions overview in the Form details tab contains an extra row for the release remaining authorization step. Also, a note is saved at the order to inform the user.
</details>
<details>
  <summary>Refund (click to expand)</summary>

To create a return in Optimizely Order Management the order must have the completed status. Follow these steps to create a return:

- Open the order in Optimizely Commerce
- Go to the Form details tab
- Click the 'Create return' button next to the line item
- New popup window is opened, add order lines, some comments and finally press 'Save'

![Order create return](/docs/screenshots/order-create-return.PNG?raw=true "Order create return")

- Press the 'Acknowledge Receipt Items' button
- To complete the return press the 'Complete button'

By default we don't include the shipping fee. This can be changed in the IKlarnaOrderService Refund method by overriding it. Note that merchants need to remember to change the total in the Complete refund screen to include the shipping fee otherwise the total will not include it.

When the return is completed the payment gateway is called to create a refund at Klarna. In the Transactions section, an extra row for the payment refund (called Credit in Optimizely) has been added. Also, a note is added to the order.

![Order payments refund](/docs/screenshots/order-payments-refund.PNG?raw=true "Order payments refund")

#### Change Refund data
It is possible to change refund data before sending it to Klarna, similar to [changing capture data](#Change-Capture-data) it is possible to do so by creating an implementation of ```IRefundBuilder``` and registering it with your IoC container.
```csharp
public class DemoRefundBuilder : IRefundBuilder
{
    public Refund Build(Refund refund, IOrderGroup orderGroup, OrderForm returnOrderForm, IPayment payment)
    {
        // Here you can make changes to refund if needed
        return refund;
    }
}
```
</details>
<details>
  <summary>Cancel (click to expand)</summary>

Whenever an order is cancelled in Optimizely the payment gateway is called to also cancel the payment in Klarna.
An order in Optimizely can only be can cancelled when the items haven't been shipped yet. 

After the cancel button is pressed the payment gateway is called. The passed payment object contains the transaction type 'Void' which means the payment should be cancelled. This is also what happens in Klarna.

![Order payments void](/docs/screenshots/order-payments-void.PNG?raw=true "Order payments void")
</details>
<details>
  <summary>Fraud</summary>

Once a Klarna order has been approved and successfully placed by a customer, the order data (customer billing address, customer shipping address, and order line items) should not be changed by a merchant admin. Updating order data after an order has been placed transfers the risk of capturing funds from Klarna to the merchant. While an order system may allow updates to an order, be aware that those updates are not updated in the Klarna system. If a change to this kind of order data is necessary, Klarna recommends cancelling the existing order and having the customer place a new Klarna order.
</details>
<details>
  <summary>Use KlarnaOrderService (click to expand)</summary>

The IKlarnaOrderService interface contains some methods to work with Klarna payments. The following methods are used for integration in Optimizely Order Management: CancelOrder, CaptureOrder, Refund and ReleaseRemainingAuthorization.

```csharp
    public interface IKlarnaOrderService
    {
        Task CancelOrder(string orderId);

        Task UpdateMerchantReferences(string orderId, string merchantReference1, string merchantReference2);
        Task<OrderManagementCapture> CaptureOrder(string orderId, int amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment);

        Task<OrderManagementCapture> CaptureOrder(string orderId, int amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment, IShipment shipment);

        Task Refund(string orderId, IOrderGroup orderGroup, OrderForm orderForm, IPayment payment);

        Task ReleaseRemainingAuthorization(string orderId);

        Task TriggerSendOut(string orderId, string captureId);

        Task<OrderManagementOrder> GetOrder(string orderId);

        Task ExtendAuthorizationTime(string orderId);

        Task UpdateCustomerInformation(string orderId, OrderManagementCustomerAddresses updateCustomerDetails);
        Task AcknowledgeOrder(IPurchaseOrder purchaseOrder);
    }
```
</details>
<details>
  <summary>Order notes (click to expand)<a name="order-notes"></a></summary>
	
Optimizely uses order notes internally to show updates to users regarding the current order. For example, when a shipment was released or when a return was created. Order notes are also saved by the Klarna package to inform merchants about the Klarna payment process. 

![Order notes](/docs/screenshots/order-notes-complete.PNG?raw=true "Order notes")
</details>

## Package maintainer
https://github.com/frederikvig

## Changelog
[Changelog](../../CHANGELOG.md)
