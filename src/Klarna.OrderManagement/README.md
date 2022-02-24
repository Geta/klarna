EPiServer Klarna order management
=============

## Description

Klarna.OrderManagement is a library for processing a Klarna payment in EPiServer Commerce. This package supports both payments of Klarna.Payments and Klarna.Checkout. 

More about Klarna ordermanagement: https://docs.klarna.com/order-management/

## Features
* Capture
* Refund
* Release remaining authorization
* Cancel
* Support multi shipment payments

### Steps integrated with EPiServer Commerce
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

(*) Not integrated in the EPiServer Commerce order process

<details>
  <summary>Setup (click to expand)</summary>

Start by installing NuGet packages (use [NuGet](http://nuget.episerver.com/)):

    Install-Package Klarna.OrderManagement.v3
    
For the Commerce Manager site run the following package:

    Install-Package Klarna.OrderManagement.CommerceManager.v3

Both Klarna.Payments and Klarna.Checkout have reference to the Klarna.OrderManagement. It's more likely that one of those packages are installed.    
</details>

<details>
  <summary>Configure user control (click to expand)</summary>
  
Unfortunately a manual configuration needs to be done in the XML file to make sure that the KlarnaPaymentControl.ascx user control is loaded in Commerce Manager. See section Klarna order information to learn what kind of information this user control displays. Follow these steps to configure the user control:
- **Open file: /Apps/Order/Config/Views/Forms/PurchaseOrder-ObjectView.xml**
- **Add the KlarnaPaymentControl.ascx to the Placeholder_2 like this**
```
<Block id="payments" name="Payments">
	<Placeholder id="Placeholder_1">
		<Control id="PaymentsGrid" path="~/Apps/Order/Modules/RelatedEntityView.ascx">
			<Property name="RelatedClassName" value="Payment" />
			<Property name="RelatedToClassName" value="Order"/>
		</Control>
	</Placeholder>
	<Placeholder id="Placeholder_2">
    <Control id="PaymentsGrid2" path="~/KlarnaSummary/KlarnaPaymentControl.ascx"></Control>
	</Placeholder>
	<Placeholder id="Placeholder_3" />
	<Placeholder id="Placeholder_4" />
</Block>
```

Note: these steps need to be done each time Commerce Manager is updated. 
</details>

<details>
  <summary>Capture (click to expand)</summary>

Capturing payments is done by completing a shipment in Commerce Manager. Follow these steps to complete a shipment:

- Open the order in CM
- Go to Order details - 'Release shipment'
- Create pick list with the order
- Go to pick lists in CM and select your picklist
- Complete shipment for corresponding order
    - You can enter tracking number in the 'Complete Shipment' pop-up, this will be available as shipping information in Klarna
    - The 'OK' button triggers the payment gateway to do a capture, if capturing the payment succeeds the pop up will close. Otherwise you'll receive an error message in the pop up or, if there's something wrong with the payment there should be a new order note with exception information on the order.

![Capture payment](/docs/screenshots/capture-complete-shipment.PNG?raw=true)

Look at the [order notes section](#order-notes) for example order notes regarding captures.


####  Partial capture (click to expand)
Upon completing a shipment in a multi-shipment scenario, a partial capture will be done towards Klarna. The partial capture will capture the amount that has to be captured for that specific shipment. If all shipments are completed, the full order amount will have been captured.

![Partial capture](/docs/screenshots/capture-partial.PNG?raw=true)


#### Change Capture data
By default all capture data should be set automatically. However, similar to Klarna Payment sessions, it is possible to change capture data before it is sent to Klarna. In order to do so you can create an implementation of ``ICaptureBuilder`` and register it with StructureMap.
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

![Order multi shipment](/docs/screenshots/order-multi-shipment.PNG?raw=true "Order multi shipment")

When the last shipment is handled, the payment gateway is called to release the remaining authorized amount at Klarna. The payments overview in the Payment tab contains an extra row for the release remaining authorization step. Also, a note is saved at the order to inform the user.

![Order release remaining authorization](/docs/screenshots/order-payment-releaseremainingauthorization.PNG?raw=true "Order release remaining authorization")
</details>
<details>
  <summary>Refund (click to expand)</summary>

To create a return in Commerce Manager the order must have the completed status. Follow these steps to create a return:

- Open the order in Commerce Manager
- Go to the Details tab
- Press the 'Create return' button
- New popup window is opened, add order lines, some comments and finally press 'Save'

![Order create return](/docs/screenshots/order-create-return.PNG?raw=true "Order create return")

- Got to the Returns tab
- Press the 'Acknowledge Receipt Items' button
- To complete the return press the 'Complete button'

When the return is completed the payment gateway is called to create a refund at Klarna. In the Payments tab, an extra row for the payment refund (called Credit in Commerce Manager) has been added. Also, a note is added at the order.

![Order payments refund](/docs/screenshots/order-payments-refund.PNG?raw=true "Order payments refund")

#### Change Refund data
It is possible to change refund data before sending it to Klarna, similar to [changing capture data](#Change-Capture-data) it is possible to do so by creating an implementation of ```IRefundBuilder``` and registering it with StructureMap.
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

Whenever an order is cancelled in Commerce Manager the payment gateway is called to also cancel the payment at Klarna.
An order in Commerce Manager can only be can cancelled when the items haven't been shipped yet. 

![Cancel order](/docs/screenshots/order-cancel.png?raw=true "Cancel order")

After the cancel button is pressed the payment gateway is called. The passed payment object contains the transaction type 'Void' which means the payment should be cancelled. This is also what happens at Klarna.

![Order payments void](/docs/screenshots/order-payments-void.PNG?raw=true "Order payments void")
</details>
<details>
  <summary>Fraud</summary>

Once a Klarna order has been approved and successfully placed by a customer, the order data (customer billing address, customer shipping address, and order line items) should not be changed by a merchant admin. Updating order data after an order has been placed transfers the risk of capturing funds from Klarna to the merchant. While an order system may allow updates to an order, be aware that those updates are not updated in the Klarna system. If a change to this kind of order data is necessary, Klarna recommends cancelling the existing order and having the customer place a new Klarna order.
</details>
<details>
  <summary>Use KlarnaOrderService (click to expand)</summary>

The IKlarnaOrderService interface contains some methods to work with Klarna payments. The following methods are used for integration in Commerce Manager: CancelOrder, CaptureOrder, Refund and ReleaseRemainingAuthorization.

```
    public interface IKlarnaOrderService
    {
        Task CancelOrder(string orderId);

        Task UpdateMerchantReferences(string orderId, string merchantReference1, string merchantReference2);
	
        Task<OrderManagementCapture> CaptureOrder(string orderId, int amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment);

        Task<OrderManagementCapture> CaptureOrder(string orderId, int amount, string description, IOrderGroup orderGroup, IOrderForm orderForm, IPayment payment, IShipment shipment);

        Task Refund(string orderId, IOrderGroup orderGroup, OrderForm orderForm, IPayment payment);

        Task ReleaseRemainingAuthorization(string orderId);

        Task TriggerSendOut(string orderId, string captureId);

        Task<PatchedOrderData> GetOrder(string orderId);

        Task ExtendAuthorizationTime(string orderId);

        Task UpdateCustomerInformation(string orderId, OrderManagementCustomerAddresses updateCustomerDetails);
	
        Task AcknowledgeOrder(IPurchaseOrder purchaseOrder);
    }
```
</details>
<details>
  <summary>Order notes (click to expand)<a name="order-notes"></a></summary>
	
EPiServer uses order notes internally to show updates to users regarding the current order. For example, when a shipment was released or when a return was created. Order notes are also saved by the Klarna package to inform users about the Klarna payment process. 

![Order notes](/docs/screenshots/order-notes-complete.PNG?raw=true "Order notes")
</details>
<details>
  <summary>Klarna order information (click to expand)</summary>

Order notes and the payment overview can be used to gather information about the Klarna payment process. The Payments tab contains more information about the order (payment) at Klarna. By clicking on the 'Show all order information' link a complete JSON of the order object from Klarna is displayed. 

Note: this information is only displayed  when a Klarna payment is added to the order in Commerce Manager.

![Klarna order information](/docs/screenshots/order-klarna-information.PNG?raw=true "Klarna order information")
</details>

## Package maintainer
https://github.com/frederikvig

## Changelog
[Changelog](../../CHANGELOG.md)
