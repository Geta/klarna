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

### Cancel

### Captur
