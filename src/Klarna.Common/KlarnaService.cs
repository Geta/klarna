using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using EPiServer.Logging;
using Klarna.Common.Configuration;
using Klarna.Common.Extensions;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Orders.Search;

namespace Klarna.Common
{
    public abstract class KlarnaService : IKlarnaService
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(KlarnaService));
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IMarketService _marketService;
        private readonly IConfigurationLoader _configurationLoader;

        protected KlarnaService(
            IOrderRepository orderRepository,
            IPaymentProcessor paymentProcessor,
            IOrderGroupCalculator orderGroupCalculator,
            IMarketService marketService, IConfigurationLoader configurationLoader)
        {
            _orderRepository = orderRepository;
            _paymentProcessor = paymentProcessor;
            _orderGroupCalculator = orderGroupCalculator;
            _marketService = marketService;
            _configurationLoader = configurationLoader;
        }

        public void FraudUpdate(NotificationModel notification)
        {
            var order = GetPurchaseOrderByKlarnaOrderId(notification.OrderId);

            if (order == null) return;

            var orderForm = order.GetFirstForm();
            var payment = orderForm.Payments.FirstOrDefault();
            if (payment == null) return;

            // Get payment method used and the configuration data
            var connectionConfiguration = _configurationLoader.GetConfiguration(payment, order.MarketId);
            string userAgent = $"Platform/Episerver.Commerce_{typeof(EPiServer.Commerce.ApplicationContext).Assembly.GetName().Version} Module/Klarna.Common_{typeof(KlarnaService).Assembly.GetName().Version}";

            var client =  new OrderManagementStore(new ApiSession
            {
                ApiUrl = connectionConfiguration.ApiUrl,
                UserAgent = userAgent,
                Credentials = new ApiCredentials
                {
                    Username = connectionConfiguration.Username,
                    Password = connectionConfiguration.Password
                }
            }, new JsonSerializer());
            
            // Make sure the order exists in Klarna
            var klarnaOrder = AsyncHelper.RunSync(() => client.GetOrder(notification.OrderId));

            if (klarnaOrder == null) return;

            // Compare fraud status of notification with Klarna order fraud status and stop process if it's still pending or doesn't match
            switch (klarnaOrder.FraudStatus)
            {
                case OrderManagementFraudStatus.ACCEPTED:
                    if (notification.Status != NotificationFraudStatus.FRAUD_RISK_ACCEPTED) return;
                    break;
                case OrderManagementFraudStatus.REJECTED:
                    if (notification.Status != NotificationFraudStatus.FRAUD_RISK_REJECTED) return;
                    break;
                case OrderManagementFraudStatus.PENDING:
                    return;
            }

            payment.Status = PaymentStatus.Pending.ToString();

            payment.Properties[Constants.FraudStatusPaymentField] = notification.Status;

            try
            {
                var result = order.ProcessPayments(_paymentProcessor, _orderGroupCalculator);
                if (result.FirstOrDefault()?.IsSuccessful == false)
                {
                    PaymentStatusManager.FailPayment((Payment)payment);
                    _orderRepository.Save(order);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            _orderRepository.Save(order);
        }

        public List<OrderLine> GetOrderLines(ICart cart, OrderGroupTotals orderGroupTotals, bool sendProductAndImageUrlField)
        {
            var market = _marketService.GetMarket(cart.MarketId);

            return GetOrderLines(cart, orderGroupTotals, market.PricesIncludeTax, sendProductAndImageUrlField);
        }

        public List<OrderLine> GetOrderLines(ICart cart, OrderGroupTotals orderGroupTotals, bool includeTaxOnLineItems, bool sendProductAndImageUrl)
        {
            return includeTaxOnLineItems
                ? GetOrderLinesWithTax(cart, orderGroupTotals, sendProductAndImageUrl)
                : GetOrderLinesWithoutTax(cart, orderGroupTotals, sendProductAndImageUrl);
        }

        private List<OrderLine> GetOrderLinesWithoutTax(ICart cart, OrderGroupTotals orderGroupTotals, bool sendProductAndImageUrl)
        {
            var shipment = cart.GetFirstShipment();
            var orderLines = new List<OrderLine>();

            // Line items
            foreach (var lineItem in cart.GetAllLineItems())
            {
                var orderLine = lineItem.GetOrderLine(sendProductAndImageUrl);
                orderLines.Add(orderLine);
            }

            // Shipment
            if (shipment != null && orderGroupTotals.ShippingTotal.Amount > 0)
            {
                var shipmentOrderLine = shipment.GetOrderLine(cart, orderGroupTotals, false);
                orderLines.Add(shipmentOrderLine);
            }

            // Sales tax
            orderLines.Add(new OrderLine()
            {
                Type = OrderLineType.sales_tax,
                Name = "Sales Tax",
                Quantity = 1,
                TotalAmount = AmountHelper.GetAmount(orderGroupTotals.TaxTotal),
                UnitPrice = AmountHelper.GetAmount(orderGroupTotals.TaxTotal),
                TotalTaxAmount = 0,
                TaxRate = 0
            });

            // Order level discounts
            var orderDiscount = cart.GetOrderDiscountTotal();

            var totalDiscount = orderDiscount.Amount;

            if (totalDiscount > 0)
            {
                orderLines.Add(new OrderLine()
                {
                    Type = OrderLineType.discount,
                    Name = "Discount",
                    Quantity = 1,
                    TotalAmount = -AmountHelper.GetAmount(totalDiscount),
                    UnitPrice = -AmountHelper.GetAmount(totalDiscount),
                    TotalTaxAmount = 0,
                    TaxRate = 0
                });
            }
            return orderLines;
        }

        private List<OrderLine> GetOrderLinesWithTax(ICart cart, OrderGroupTotals orderGroupTotals, bool sendProductAndImageUrl)
        {
            var shipment = cart.GetFirstShipment();
            var orderLines = new List<OrderLine>();
            var market = _marketService.GetMarket(cart.MarketId);

            // Line items
            foreach (var lineItem in cart.GetAllLineItems())
            {
                var orderLine = lineItem.GetOrderLineWithTax(market, cart.GetFirstShipment(), cart.Currency, sendProductAndImageUrl);
                orderLines.Add(orderLine);
            }

            // Shipment
            if (shipment != null && orderGroupTotals.ShippingTotal.Amount > 0)
            {
                var shipmentOrderLine = shipment.GetOrderLine(cart, orderGroupTotals, true);
                orderLines.Add(shipmentOrderLine);
            }

            // Without tax
            var orderLevelDiscount = AmountHelper.GetAmount(cart.GetOrderDiscountTotal());
            if (orderLevelDiscount > 0)
            {
                // Order level discounts with tax
                var totalOrderAmountWithoutDiscount =
                    orderLines
                        .Sum(x => x.TotalAmount);
                var totalOrderAmountWithDiscount = AmountHelper.GetAmount(orderGroupTotals.Total.Amount);
                var orderLevelDiscountIncludingTax = totalOrderAmountWithoutDiscount - totalOrderAmountWithDiscount;

                // Tax
                var totalTaxAmountWithoutDiscount =
                    orderLines
                        .Sum(x => x.TotalTaxAmount);
                var totalTaxAmountWithDiscount = AmountHelper.GetAmount(orderGroupTotals.TaxTotal);
                var discountTax = totalTaxAmountWithoutDiscount - totalTaxAmountWithDiscount;
                var taxRate = discountTax * 100 / (orderLevelDiscountIncludingTax - discountTax);

                orderLines.Add(new OrderLine()
                {
                    Type = OrderLineType.discount,
                    Name = "Discount",
                    Quantity = 1,
                    TotalAmount = orderLevelDiscountIncludingTax * -1,
                    UnitPrice = orderLevelDiscountIncludingTax * -1,
                    TotalTaxAmount = discountTax * -1,
                    TaxRate = AmountHelper.GetAmount(taxRate)
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
                Classes = new System.Collections.Specialized.StringCollection { "PurchaseOrder" },
                Namespace = "Mediachase.Commerce.Orders"
            };

            var parameters = new OrderSearchParameters();
            parameters.SqlMetaWhereClause = $"META.{Constants.KlarnaOrderIdField} LIKE '{orderId}'";


            var purchaseOrder = OrderContext.Current.Search<PurchaseOrder>(parameters, searchOptions)?.FirstOrDefault();

            if (purchaseOrder != null)
            {
                return _orderRepository.Load<IPurchaseOrder>(purchaseOrder.OrderGroupId);
            }
            return null;
        }
    }
}
