using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    internal class OrderInitialization : IInitializableModule
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(OrderInitialization));
        private Injected<IOrderRepository> _orderRepository;
        private Injected<IOrderGroupFactory> _orderGroupFactory;
        private Injected<IPaymentProcessor> _paymentProcessor;

        public void Initialize(InitializationEngine context)
        {
            OrderContext.Current.OrderGroupUpdated += Current_OrderGroupUpdated;
        }

        private void Current_OrderGroupUpdated(object sender, OrderGroupEventArgs e)
        {
            if (e.OrderGroupType == OrderGroupEventType.PurchaseOrder)
            {
                var orderAfterSave = sender as IPurchaseOrder;
                var orderBeforeSave = _orderRepository.Service.Load<IPurchaseOrder>(e.OrderGroupId);
                if (orderBeforeSave != null && orderAfterSave != null)
                {
                    // multi shipment scenario. Call payment gateway to release remaining authorization
                    if (orderBeforeSave.OrderStatus == OrderStatus.PartiallyShipped && orderAfterSave.OrderStatus == OrderStatus.Completed && orderAfterSave.GetFirstForm().Shipments.Any(s => s.OrderShipmentStatus == OrderShipmentStatus.Cancelled))
                    {
                        if (orderAfterSave.GetFirstForm().Payments.All(p => p.TransactionType != KlarnaAdditionalTransactionType.ReleaseRemainingAuthorization.ToString()))
                        {
                            var previousPayment = orderAfterSave.GetFirstForm().Payments.FirstOrDefault();
                            if (previousPayment != null)
                            {
                                var payment = orderAfterSave.CreatePayment(_orderGroupFactory.Service);
                                payment.PaymentType = previousPayment.PaymentType;
                                payment.PaymentMethodId = previousPayment.PaymentMethodId;
                                payment.PaymentMethodName = previousPayment.PaymentMethodName;

                                var remainingAmount = orderAfterSave.GetFirstForm().Shipments.Where(s => s.OrderShipmentStatus != OrderShipmentStatus.Shipped).Sum(x => (x.GetShippingItemsTotal(orderAfterSave.Currency).Amount + x.GetShippingCost(orderAfterSave.Market, orderAfterSave.Currency).Amount));

                                payment.Amount = remainingAmount;
                                payment.Status = PaymentStatus.Pending.ToString();
                                payment.TransactionType = KlarnaAdditionalTransactionType.ReleaseRemainingAuthorization.ToString();

                                orderAfterSave.AddPayment(payment);

                                _paymentProcessor.Service.ProcessPayment(orderAfterSave, payment, orderAfterSave.GetFirstShipment());
                            }
                        }
                    }
                    else if (orderBeforeSave.OrderStatus != OrderStatus.Cancelled && orderAfterSave.OrderStatus == OrderStatus.Cancelled)
                    { 
                        if (orderAfterSave.GetFirstForm().Payments.All(p => p.TransactionType != TransactionType.Void.ToString() && p.TransactionType != TransactionType.Capture.ToString()))
                        {
                            var previousPayment = orderAfterSave.GetFirstForm().Payments.FirstOrDefault();
                            if (previousPayment != null)
                            {
                                var payment = orderAfterSave.CreatePayment(_orderGroupFactory.Service);
                                payment.PaymentType = previousPayment.PaymentType;
                                payment.PaymentMethodId = previousPayment.PaymentMethodId;
                                payment.PaymentMethodName = previousPayment.PaymentMethodName;
                                payment.Amount = previousPayment.Amount;
                                payment.Status = PaymentStatus.Pending.ToString();
                                payment.TransactionType = TransactionType.Void.ToString();

                                orderAfterSave.AddPayment(payment);

                                _paymentProcessor.Service.ProcessPayment(orderAfterSave, payment, orderAfterSave.GetFirstShipment());
                            }
                        }
                    }
                }
            }
        }

        public void Uninitialize(InitializationEngine context)
        {

        }
    }
}
