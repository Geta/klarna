using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Klarna.Common.Extensions;
using Mediachase.Commerce.Markets;
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

    [ServiceConfiguration(typeof(ReleaseRemainingEventHandler))]
    public class ReleaseRemainingEventHandler
    {
        private IPurchaseOrder _order;
        private IOrderForm _orderForm;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly IMarketService _marketService;

        public ReleaseRemainingEventHandler(
            IPaymentProcessor paymentProcessor,
            IOrderGroupFactory orderGroupFactory,
            IMarketService marketService)
        {
            _paymentProcessor = paymentProcessor ?? throw new ArgumentNullException(nameof(paymentProcessor));
            _orderGroupFactory = orderGroupFactory ?? throw new ArgumentNullException(nameof(orderGroupFactory));
            _marketService = marketService ?? throw new ArgumentNullException(nameof(marketService));
        }

        public void Handle(ReleaseRemainingEvent ev)
        {
            _order = ev.PurchaseOrder;
            _orderForm = _order.GetFirstForm();
            var market = _marketService.GetMarket(_order.MarketId);

            var previousPayment =
                _orderForm.Payments.FirstOrDefault(x => x.IsKlarnaPayment());
            if (previousPayment == null) return;

            var payment = _order.CreatePayment(_orderGroupFactory);
            payment.PaymentType = previousPayment.PaymentType;
            payment.PaymentMethodId = previousPayment.PaymentMethodId;
            payment.PaymentMethodName = previousPayment.PaymentMethodName;

            var remainingAmount = _orderForm
                .Shipments
                .Where(s => s.OrderShipmentStatus != OrderShipmentStatus.Shipped)
                .Sum(x => x.GetShippingItemsTotal(_order.Currency).Amount + x.GetShippingCost(market, _order.Currency).Amount);

            payment.Amount = remainingAmount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = KlarnaAdditionalTransactionType.ReleaseRemainingAuthorization.ToString();

            _order.AddPayment(payment);

            _paymentProcessor.ProcessPayment(_order, payment, _order.GetFirstShipment());
        }
    }
}