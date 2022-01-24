using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Klarna.OrderManagement.Events;
using Mediachase.Commerce.Orders;

namespace Klarna.OrderManagement.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    internal class OrderInitialization : IInitializableModule
    {
        private IServiceProvider _locator;

        public void Initialize(InitializationEngine context)
        {
            _locator = context.Locate.Advanced;
            OrderContext.Current.OrderGroupUpdated += Current_OrderGroupUpdated;
        }

        private void Current_OrderGroupUpdated(object sender, OrderGroupEventArgs e)
        {
            var orderRepository = _locator.GetInstance<IOrderRepository>();
            if (e.OrderGroupType != OrderGroupEventType.PurchaseOrder) return;

            var orderAfterSave = sender as IPurchaseOrder;
            var orderBeforeSave = orderRepository.Load<IPurchaseOrder>(e.OrderGroupId);
            if (orderBeforeSave == null || orderAfterSave == null) return;

            // multi shipment scenario. Call payment gateway to release remaining authorization
            if (IsReleaseRemaining(orderBeforeSave, orderAfterSave))
            {
                var reseaseRemainingEventHandler = _locator.GetInstance<ReleaseRemainingEventHandler>();
                reseaseRemainingEventHandler.Handle(new ReleaseRemainingEvent(orderAfterSave));
            }

            if (orderAfterSave.OrderStatus == OrderStatus.Cancelled)
            {
                var orderCancelledEventHandler = _locator.GetInstance<OrderCancelledEventHandler>();
                orderCancelledEventHandler.Handle(new OrderCancelledEvent(orderAfterSave));
            }
        }

        private static bool IsReleaseRemaining(IPurchaseOrder orderBeforeSave, IPurchaseOrder orderAfterSave)
        {
            var form = orderAfterSave.GetFirstForm();
            return orderBeforeSave.OrderStatus == OrderStatus.PartiallyShipped
                   && orderAfterSave.OrderStatus == OrderStatus.Completed
                   && form.Shipments.Any(s => s.OrderShipmentStatus == OrderShipmentStatus.Cancelled)
                   && form.Payments.All(p => p.TransactionType != KlarnaAdditionalTransactionType.ReleaseRemainingAuthorization.ToString());
        }

        public void Uninitialize(InitializationEngine context)
        {

        }
    }
}
