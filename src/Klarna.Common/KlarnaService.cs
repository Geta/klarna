using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using EPiServer.Logging;
using Klarna.Common.Extensions;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Klarna.Rest.Models;
using Mediachase.Commerce.Orders.Search;

namespace Klarna.Common
{
    public abstract class KlarnaService : IKlarnaService
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(KlarnaService));
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IOrderGroupCalculator _orderGroupCalculator;

        protected KlarnaService(IOrderRepository orderRepository, IPaymentProcessor paymentProcessor, IOrderGroupCalculator orderGroupCalculator)
        {
            _orderRepository = orderRepository;
            _paymentProcessor = paymentProcessor;
            _orderGroupCalculator = orderGroupCalculator;
        }

        public void FraudUpdate(NotificationModel notification)
        {
            var order = GetPurchaseOrderByKlarnaOrderId(notification.OrderId);
            if (order != null)
            {
                var orderForm = order.GetFirstForm();
                var payment = orderForm.Payments.FirstOrDefault();
                if (payment != null && payment.Status == PaymentStatus.Pending.ToString())
                {
                    payment.Properties[Constants.FraudStatusPaymentMethodField] = notification.Status.ToString();

                    try
                    {
                        order.ProcessPayments(_paymentProcessor, _orderGroupCalculator);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message, ex);
                    }
                    _orderRepository.Save(order);
                }
            }
        }

        public List<OrderLine> GetOrderLines(ICart cart, OrderGroupTotals orderGroupTotals)
        {
            var shipment = cart.GetFirstShipment();
            var orderLines = new List<OrderLine>();

            var includedTaxesOnLineItems = CountryCodeHelper.GetContinentByCountry(shipment.ShippingAddress?.CountryCode).Equals("NA", StringComparison.InvariantCultureIgnoreCase);

            // Line items
            foreach (var lineItem in cart.GetAllLineItems())
            {
                var orderLine = lineItem.GetOrderLine(cart.Market, cart.GetFirstShipment(), cart.Currency);
                orderLines.Add(orderLine);
            }

            // Shipment
            if (shipment != null && orderGroupTotals.ShippingTotal.Amount > 0)
            {
                var shipmentOrderLine = shipment.GetOrderLine(cart, orderGroupTotals, includedTaxesOnLineItems);
                orderLines.Add(shipmentOrderLine);
            }

            if (!includedTaxesOnLineItems)
            {
                // Sales tax
                orderLines.Add(new PatchedOrderLine()
                {
                    Type = "sales_tax",
                    Name = "Sales Tax",
                    Quantity = 1,
                    TotalAmount = AmountHelper.GetAmount(orderGroupTotals.TaxTotal),
                    UnitPrice = AmountHelper.GetAmount(orderGroupTotals.TaxTotal),
                    TotalTaxAmount = 0,
                    TaxRate = 0
                });
            }
            return orderLines;
        }

        
        public IPurchaseOrder GetPurchaseOrderByKlarnaOrderId(string orderId)
        {
            OrderSearchOptions searchOptions = new OrderSearchOptions
            {
                CacheResults = false,
                StartingRecord = 0,
                RecordsToRetrieve = 1,
                Classes = new System.Collections.Specialized.StringCollection {"PurchaseOrder"},
                Namespace = "Mediachase.Commerce.Orders"
            };

            var parameters = new OrderSearchParameters();
            parameters.SqlMetaWhereClause = $"META.{Constants.KlarnaOrderIdField} LIKE '{orderId}'";

            var purchaseOrder = OrderContext.Current.FindPurchaseOrders(parameters, searchOptions)?.FirstOrDefault();

            if (purchaseOrder != null)
            {
                return _orderRepository.Load<IPurchaseOrder>(purchaseOrder.OrderGroupId);
            }
            return null;
        }
    }
}
