using System;
using System.Linq;
using EPiServer.Commerce.Order;
using Klarna.Common;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement.Events
{
    public class ReleaseRemainingEvent
    {
        public ReleaseRemainingEvent(IPurchaseOrder purchaseOrder)
        {
            PurchaseOrder = purchaseOrder ?? throw new ArgumentNullException(nameof(purchaseOrder));
        }

        public IPurchaseOrder PurchaseOrder { get; }
    }

    public class ReleaseRemainingEventHandler
    {
        private IPurchaseOrder _order;
        private IOrderForm _orderForm;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IOrderGroupFactory _orderGroupFactory;

        public ReleaseRemainingEventHandler(
            IPaymentProcessor paymentProcessor,
            IOrderGroupFactory orderGroupFactory)
        {
            _paymentProcessor = paymentProcessor ?? throw new ArgumentNullException(nameof(paymentProcessor));
            _orderGroupFactory = orderGroupFactory ?? throw new ArgumentNullException(nameof(orderGroupFactory));
        }

        public void Handle(ReleaseRemainingEvent ev)
        {
            _order = ev.PurchaseOrder;
            _orderForm = _order.GetFirstForm();

            var previousPayment = _orderForm.Payments.FirstOrDefault(x => x.PaymentMethodName.Equals(Constants.KlarnaPaymentSystemKeyword));
            if (previousPayment == null) return;

            var payment = _order.CreatePayment(_orderGroupFactory);
            payment.PaymentType = previousPayment.PaymentType;
            payment.PaymentMethodId = previousPayment.PaymentMethodId;
            payment.PaymentMethodName = previousPayment.PaymentMethodName;

            var remainingAmount = _orderForm
                .Shipments
                .Where(s => s.OrderShipmentStatus != OrderShipmentStatus.Shipped)
                .Sum(x => x.GetShippingItemsTotal(_order.Currency).Amount + x.GetShippingCost(_order.Market, _order.Currency).Amount);

            payment.Amount = remainingAmount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = KlarnaAdditionalTransactionType.ReleaseRemainingAuthorization.ToString();

            _order.AddPayment(payment);

            _paymentProcessor.ProcessPayment(_order, payment, _order.GetFirstShipment());
        }
    }
}