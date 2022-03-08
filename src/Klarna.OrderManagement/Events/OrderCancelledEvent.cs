using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Klarna.Common.Extensions;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement.Events
{
    public class OrderCancelledEvent
    {
        public OrderCancelledEvent(IPurchaseOrder purchaseOrder)
        {
            PurchaseOrder = purchaseOrder ?? throw new ArgumentNullException(nameof(purchaseOrder));
        }

        public IPurchaseOrder PurchaseOrder { get; }
    }

    [ServiceConfiguration(typeof(OrderCancelledEventHandler))]
    public class OrderCancelledEventHandler
    {
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private IPurchaseOrder _order;
        private IOrderForm _orderForm;

        public OrderCancelledEventHandler(
            IPaymentProcessor paymentProcessor,
            IOrderGroupFactory orderGroupFactory)
        {
            _paymentProcessor = paymentProcessor ?? throw new ArgumentNullException(nameof(paymentProcessor));
            _orderGroupFactory = orderGroupFactory ?? throw new ArgumentNullException(nameof(orderGroupFactory));
        }

        public void Handle(OrderCancelledEvent ev)
        {
            _order = ev.PurchaseOrder;
            _orderForm = _order.GetFirstForm();

            if (AlreadyVoided()) return;

            var previousPayment = _orderForm.Payments.FirstOrDefault(x => x.IsKlarnaPayment());
            if (previousPayment == null) return;

            var voidPayment = _order.CreatePayment(_orderGroupFactory);
            voidPayment.PaymentType = previousPayment.PaymentType;
            voidPayment.PaymentMethodId = previousPayment.PaymentMethodId;
            voidPayment.PaymentMethodName = previousPayment.PaymentMethodName;
            voidPayment.Amount = previousPayment.Amount;
            voidPayment.Status = PaymentStatus.Pending.ToString();
            voidPayment.TransactionType = TransactionType.Void.ToString();

            _order.AddPayment(voidPayment);

            _paymentProcessor.ProcessPayment(_order, voidPayment, _order.GetFirstShipment());
        }

        private bool AlreadyVoided()
        {
            return _orderForm.Payments.Any(
                p => p.TransactionType == TransactionType.Void.ToString()
                     || p.TransactionType == TransactionType.Capture.ToString());
        }
    }
}