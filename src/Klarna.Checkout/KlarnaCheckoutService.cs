using System;
using System.Collections.Generic;
using System.Net;
using EPiServer;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using EPiServer.Logging;
using EPiServer.Web.Routing;
using Klarna.Common;
using Klarna.Rest;
using Klarna.Rest.Checkout;
using Klarna.Rest.Models;
using Klarna.Rest.Transport;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders.Managers;

namespace Klarna.Checkout
{
    [ServiceConfiguration(typeof(IKlarnaCheckoutService))]
    public class KlarnaCheckoutService : IKlarnaCheckoutService
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(KlarnaCheckoutService));
        private readonly IOrderGroupTotalsCalculator _orderGroupTotalsCalculator;
        
        private readonly IOrderRepository _orderRepository;
        private readonly ReferenceConverter _referenceConverter;
        private readonly UrlResolver _urlResolver;
        private readonly IContentRepository _contentRepository;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IConnectionFactory _connectionFactory;

        private Client _client;

        public KlarnaCheckoutService(
            IOrderGroupTotalsCalculator orderGroupTotalsCalculator,
            IOrderRepository orderRepository,
            ReferenceConverter referenceConverter,
            UrlResolver urlResolver,
            IContentRepository contentRepository,
            IOrderNumberGenerator orderNumberGenerator,
            IPaymentProcessor paymentProcessor,
            IOrderGroupCalculator orderGroupCalculator,
            IConnectionFactory connectionFactory)
        {
            _orderGroupTotalsCalculator = orderGroupTotalsCalculator;
            _orderRepository = orderRepository;
            _referenceConverter = referenceConverter;
            _urlResolver = urlResolver;
            _contentRepository = contentRepository;
            _orderNumberGenerator = orderNumberGenerator;
            _paymentProcessor = paymentProcessor;
            _orderGroupCalculator = orderGroupCalculator;
            _connectionFactory = connectionFactory;
        }

        public Client Client
        {
            get
            {
                if (_client == null)
                {
                    var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaCheckoutSystemKeyword, ContentLanguage.PreferredCulture.Name);
                    if (paymentMethod != null)
                    {
                        var connectionConfiguration = _connectionFactory.GetConnectionConfiguration(paymentMethod);
                        var connector = ConnectorFactory.Create(connectionConfiguration.Username, connectionConfiguration.Password, new Uri(connectionConfiguration.ApiUrl));

                        _client = new Client(connector);
                    }
                }
                return _client;
            }
        }

        public CheckoutOrderData CreateOrUpdateSession(ICart cart)
        {
            if (string.IsNullOrWhiteSpace(cart.Properties["test"]?.ToString()))
            {
                return CreateOrder(cart);
            }
            else
            {
                return UpdateOrder(cart);
            }
        }

        public CheckoutOrderData GetOrder(ICart cart)
        {
            var orderID = "12345";
            var order = Client.NewCheckoutOrder(orderID);

            try
            {
                var orderData = order.Fetch();
                return orderData;
            }
            catch (ApiException ex)
            {
                Console.WriteLine(ex.ErrorMessage.ErrorCode);
                Console.WriteLine(ex.ErrorMessage.ErrorMessages);
                Console.WriteLine(ex.ErrorMessage.CorrelationId);
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public CheckoutOrderData CreateOrder(ICart cart)
        {
            var checkout = Client.NewCheckoutOrder();

            var orderLine = new OrderLine
            {
                Type = "physical",
                Reference = "123050",
                Name = "Tomatoes",
                Quantity = 10,
                QuantityUnit = "kg",
                UnitPrice = 600,
                TaxRate = 2500,
                TotalAmount = 6000,
                TotalTaxAmount = 1200
            };

            var orderLine2 = new OrderLine
            {
                Type = "physical",
                Reference = "543670",
                Name = "Bananas",
                Quantity = 1,
                QuantityUnit = "bag",
                UnitPrice = 5000,
                TaxRate = 2500,
                TotalAmount = 4000,
                TotalDiscountAmount = 1000,
                TotalTaxAmount = 800
            };

            var merchantUrls = new MerchantUrls
            {
                Terms = new Uri("http://www.merchant.com/toc"),
                Checkout = new Uri("http://www.merchant.com/checkout?klarna_order_id={checkout.order.id}"),
                Confirmation = new Uri("http://www.merchant.com/thank-you?klarna_order_id={checkout.order.id}"),
                Push = new Uri("http://www.merchant.com/create_order?klarna_order_id={checkout.order.id}")
            };

            var orderData = new CheckoutOrderData()
            {
                PurchaseCountry = "gb",
                PurchaseCurrency = "gbp",
                Locale = "en-gb",
                OrderAmount = 10000,
                OrderTaxAmount = 2000,
                OrderLines = new List<OrderLine> { orderLine, orderLine2 },
                MerchantUrls = merchantUrls
            };

            try
            {
                checkout.Create(orderData);
                orderData = checkout.Fetch();

                var orderID = orderData.OrderId;
                return orderData;
            }
            catch (ApiException ex)
            {
                Console.WriteLine(ex.ErrorMessage.ErrorCode);
                Console.WriteLine(ex.ErrorMessage.ErrorMessages);
                Console.WriteLine(ex.ErrorMessage.CorrelationId);
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public CheckoutOrderData UpdateOrder(ICart cart)
        {
            var orderID = "12345";

            var checkout = Client.NewCheckoutOrder(orderID);

            var orderData = new CheckoutOrderData
            {
                OrderAmount = 11000,
                OrderTaxAmount = 2200
            };

            var lines = new
                List<OrderLine>
                {
                    new OrderLine()
                    {
                        Type = "physical",
                        Reference = "123050",
                        Name = "Tomatoes",
                        Quantity = 10,
                        QuantityUnit = "kg",
                        UnitPrice = 600,
                        TaxRate = 2500,
                        TotalAmount = 6000,
                        TotalTaxAmount = 1200
                    },
                    new OrderLine()
                    {
                        Type = "physical",
                        Reference = "543670",
                        Name = "Bananas",
                        Quantity = 1,
                        QuantityUnit = "bag",
                        UnitPrice = 5000,
                        TaxRate = 2500,
                        TotalAmount = 4000,
                        TotalDiscountAmount = 1000,
                        TotalTaxAmount = 800
                    },
                    new OrderLine()
                    {
                        Type = "shipping_fee",
                        Name = "Express delivery",
                        Quantity = 1,
                        UnitPrice = 1000,
                        TaxRate = 2500,
                        TotalAmount = 1000,
                        TotalTaxAmount = 200
                    }
                };

            orderData.OrderLines = lines;

            try
            {
                orderData = checkout.Update(orderData);
                return orderData;
            }
            catch (ApiException ex)
            {
                Console.WriteLine(ex.ErrorMessage.ErrorCode);
                Console.WriteLine(ex.ErrorMessage.ErrorMessages);
                Console.WriteLine(ex.ErrorMessage.CorrelationId);
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }
    }
}

