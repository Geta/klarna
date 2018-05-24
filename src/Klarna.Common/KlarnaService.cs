using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using EPiServer.Logging;
using Klarna.Common.Extensions;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Klarna.Payments.Models;
using Klarna.Rest.Models;
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

        protected KlarnaService(
            IOrderRepository orderRepository,
            IPaymentProcessor paymentProcessor,
            IOrderGroupCalculator orderGroupCalculator,
            IMarketService marketService)
        {
            _orderRepository = orderRepository;
            _paymentProcessor = paymentProcessor;
            _orderGroupCalculator = orderGroupCalculator;
            _marketService = marketService;
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
                    payment.Properties[Constants.FraudStatusPaymentField] = notification.Status.ToString();

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
            }
        }

        public List<OrderLine> GetOrderLines(ICart cart, OrderGroupTotals orderGroupTotals, bool sendProductAndImageUrlField)
        {
            var shipment = cart.GetFirstShipment();
            var market = _marketService.GetMarket(cart.MarketId);
            var currentCountry = shipment.ShippingAddress?.CountryCode ?? market.Countries.FirstOrDefault();

            var includedTaxesOnLineItems = !CountryCodeHelper.GetContinentByCountry(currentCountry).Equals("NA", StringComparison.InvariantCultureIgnoreCase);
            return GetOrderLines(cart, orderGroupTotals, includedTaxesOnLineItems, sendProductAndImageUrlField);
        }

        public List<OrderLine> GetOrderLines(ICart cart, OrderGroupTotals orderGroupTotals, bool includeTaxOnLineItems, bool sendProductAndImageUrl)
        {
            return includeTaxOnLineItems ? GetOrderLinesWithTax(cart, orderGroupTotals, sendProductAndImageUrl) : GetOrderLinesWithoutTax(cart, orderGroupTotals, sendProductAndImageUrl);
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

            // Order level discounts
            var orderDiscount = cart.GetOrderDiscountTotal();
            var entryLevelDiscount = cart.GetAllLineItems().Sum(x => x.GetEntryDiscount());
            var totalDiscount = orderDiscount.Amount + entryLevelDiscount;
            if (totalDiscount > 0)
            {
                orderLines.Add(new PatchedOrderLine()
                {
                    Type = "discount",
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
                var totalOrderAmountWithoutDiscount = orderLines.Where(x => x.TotalAmount.HasValue).Sum(x => x.TotalAmount.Value);
                var totalOrderAmountWithDiscount = AmountHelper.GetAmount(orderGroupTotals.Total.Amount);
                var orderLevelDiscountIncludingTax = totalOrderAmountWithoutDiscount - totalOrderAmountWithDiscount;

                // Tax
                var discountTax = (orderLevelDiscountIncludingTax - orderLevelDiscount);

                orderLines.Add(new PatchedOrderLine()
                {
                    Type = "discount",
                    Name = "Discount",
                    Quantity = 1,
                    TotalAmount = orderLevelDiscountIncludingTax * -1,
                    UnitPrice = orderLevelDiscountIncludingTax * -1,
                    TotalTaxAmount = discountTax * -1,
                    TaxRate = AmountHelper.GetAmount(((decimal) discountTax) / orderLevelDiscount * 100)
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

            var purchaseOrder = OrderContext.Current.FindPurchaseOrders(parameters, searchOptions)?.FirstOrDefault();

            if (purchaseOrder != null)
            {
                return _orderRepository.Load<IPurchaseOrder>(purchaseOrder.OrderGroupId);
            }
            return null;
        }
    }
}
