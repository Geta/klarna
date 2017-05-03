EPiServer Klarna order management
=============

## What is Klarna.OrderManagement?

Klarna.OrderManagement is a library for processing a Klarna payment in EPiServer Commerce. This package supports both payments of Klarna.Payments and Klarna.Checkout. 
More about Klarna ordermanagement: https://developers.klarna.com/en/gb/kco-v3/order-management

### Steps integrated with EPiServer Commerce
- **Capture** - either partially (multi-shipment) or full capture the payment amount
- **Release remaining authorization** - release remaining authorization when the payment amount has not been fully captured
- **Refund** - either partially or full refund an amount
- **Cancel** - cancel payment

### Other steps only available in the (*)code
- **Get Klarna order**
- **Update merchant reference** - update merchant reference 1 and 2
- **Trigger send out**
- **Extend authorization time**
- **Update customer information**

(*) not integrated in EPiServer Commerce

### Capture

### Release remaining authorization

### Refund
To create a return in Commerce Manager the order must have the completed status. Follow these steps to create a return:
- Open the order in Commerce Manager
- Go to the Details tab
- Press the 'Create return' button
- New popup window is opened, add order lines, some comments and finally press 'Save'
![Order create return](/docs/screenshots/order-create-return.PNG?raw=true "Order create return")
- Got ot the Returns tab
- Press the 'Acknowledge Receipt Items' button
- To complete the return press the 'Complete button'

When the return is completed the payment gateway is called to create a refund at Klarna. In the Payments tab, an extra row for the payment refund (called Credit in Commerce Manager) has been added. Also, a note add the order is created.

![Order payments refund](/docs/screenshots/order-payments-refund.PNG?raw=true "Order payments refund")
![Order notes refund](/docs/screenshots/order-notes-refund.PNG?raw=true "Order notes refund")

### Cancel

### Captur
