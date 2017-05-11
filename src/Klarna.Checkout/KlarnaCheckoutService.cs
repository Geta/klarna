using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using EPiServer.Logging;
using Klarna.Checkout.Models;
using Klarna.Common;
using Klarna.Common.Extensions;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Klarna.Rest;
using Klarna.Rest.Models;
using Klarna.Rest.Transport;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Orders.Search;

namespace Klarna.Checkout
{
    [ServiceConfiguration(typeof(IKlarnaCheckoutService))]
    public class KlarnaCheckoutService : KlarnaService, IKlarnaCheckoutService
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(KlarnaCheckoutService));
        private readonly IOrderGroupTotalsCalculator _orderGroupTotalsCalculator;

        private readonly IOrderRepository _orderRepository;
        private readonly ITaxCalculator _taxCalculator;
        private readonly IConnectionFactory _connectionFactory;

        private Client _client;

        public KlarnaCheckoutService(
            IOrderGroupTotalsCalculator orderGroupTotalsCalculator,
            IOrderRepository orderRepository,
            IConnectionFactory connectionFactory,
            ITaxCalculator taxCalculator,
            IPaymentProcessor paymentProcessor,
            IOrderGroupCalculator orderGroupCalculator) : base(orderRepository, paymentProcessor, orderGroupCalculator)
        {
            _orderGroupTotalsCalculator = orderGroupTotalsCalculator;
            _orderRepository = orderRepository;
            _connectionFactory = connectionFactory;
            _taxCalculator = taxCalculator;
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
                        var connector = ConnectorFactory.Create(connectionConfiguration.Username,
                            connectionConfiguration.Password, new Uri(connectionConfiguration.ApiUrl));

                        _client = new Client(connector);
                    }
                }
                return _client;
            }
        }

        public CheckoutOrderData CreateOrUpdateOrder(ICart cart)
        {
            var orderId = cart.Properties[Constants.KlarnaCheckoutOrderIdField]?.ToString();
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return CreateOrder(cart);
            }
            else
            {
                return UpdateOrder(orderId, cart);
            }
        }

        public CheckoutOrderData CreateOrder(ICart cart)
        {
            var checkout = Client.NewCheckoutOrder();
            var orderData = GetCheckoutOrderData(cart);

            try
            {
                checkout.Create(orderData);
                orderData = checkout.Fetch();

                // Store checkout order id on cart
                cart.Properties[Constants.KlarnaCheckoutOrderIdField] = orderData.OrderId;
                _orderRepository.Save(cart);

                return orderData;
            }
            //TODO handle exceptions
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

        public CheckoutOrderData UpdateOrder(string orderId, ICart cart)
        {
            var checkout = Client.NewCheckoutOrder(orderId);
            var orderData = GetCheckoutOrderData(cart);

            try
            {
                orderData = checkout.Update(orderData);
                // TODO check pre-set data (update cart?)
                var shipment = cart.GetFirstShipment();
                if (shipment != null)
                {
                    shipment.ShippingAddress = orderData.ShippingAddress.ToOrderAddress(cart);
                }
                return orderData;
            }
            //TODO handle exceptions
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

        private CheckoutOrderData GetCheckoutOrderData(ICart cart)
        {
            var totals = _orderGroupTotalsCalculator.GetTotals(cart);
            var orderData = new PatchedCheckoutOrderData
            {
                ShippingCountries = GetCountries().ToList(),
                PurchaseCountry = CountryCodeHelper.GetTwoLetterCountryCode(cart.Market.Countries.FirstOrDefault()),
                PurchaseCurrency = cart.Currency.CurrencyCode,
                Locale = ContentLanguage.PreferredCulture.Name,
                // Non-negative, minor units. Total amount of the order, including tax and any discounts.
                OrderAmount = AmountHelper.GetAmount(totals.Total),
                // Non-negative, minor units. The total tax amount of the order.
                OrderTaxAmount = AmountHelper.GetAmount(totals.TaxTotal),
                ShippingOptions = GetShippingOptions(cart),
                MerchantUrls = GetMerchantUrls(cart),
                OrderLines = GetOrderLines(cart)
            } as CheckoutOrderData;
            return orderData;
        }

        public CheckoutOrderData GetOrder(string orderId)
        {
            var checkout = Client.NewCheckoutOrder(orderId);

            return checkout.Fetch();
        }

        public ICart GetCartByKlarnaOrderId(int orderGroupdId, string orderId)
        {
            //var checkoutOrderData = GetOrder(orderId);

            var cart = _orderRepository.Load<ICart>(orderGroupdId);

            //if (cart.Properties[Constants.KlarnaCheckoutOrderIdField]?.ToString() == orderId)
            //{
            return cart;
            //}
            return null;
        }

        public ShippingOptionUpdateResponse UpdateShippingMethod(ICart cart, ShippingOptionUpdateRequest shippingOptionUpdateRequest)
        {
            var shipment = cart.GetFirstShipment();
            if (shipment != null && Guid.TryParse(shippingOptionUpdateRequest.SelectedShippingOption.Id, out Guid guid))
            {
                shipment.ShippingMethodId = guid;
                shipment.ShippingAddress = shippingOptionUpdateRequest.ShippingAddress.ToOrderAddress(cart);
                _orderRepository.Save(cart);
            }

            var totals = _orderGroupTotalsCalculator.GetTotals(cart);
            return new ShippingOptionUpdateResponse
            {
                OrderAmount = AmountHelper.GetAmount(totals.Total),
                OrderTaxAmount = AmountHelper.GetAmount(totals.TaxTotal),
                OrderLines = GetOrderLines(cart),
                PurchaseCurrency = cart.Currency.CurrencyCode
            };
        }

        public AddressUpdateResponse UpdateAddress(ICart cart, AddressUpdateRequest addressUpdateRequest)
        {
            if (addressUpdateRequest.SelectedShippingOption != null && Guid.TryParse(addressUpdateRequest.SelectedShippingOption.Id, out Guid shippingMethodGuid))
            {
                var shipment = cart.GetFirstForm().Shipments.FirstOrDefault(s => s.ShippingMethodId == shippingMethodGuid);
                if (shipment != null)
                {
                    shipment.ShippingAddress = addressUpdateRequest.ShippingAddress.ToOrderAddress(cart);
                }
                _orderRepository.Save(cart);
            }

            var totals = _orderGroupTotalsCalculator.GetTotals(cart);
            return new AddressUpdateResponse
            {
                OrderAmount = AmountHelper.GetAmount(totals.Total),
                OrderTaxAmount = AmountHelper.GetAmount(totals.TaxTotal),
                OrderLines = GetOrderLines(cart),
                PurchaseCurrency = cart.Currency.CurrencyCode,
                ShippingOptions = GetShippingOptions(cart)
            };
        }

        public ErrorResult ValidateOrder(ICart cart, PatchedCheckoutOrderData checkoutData)
        {
            var result = new ErrorResult();
            result.ErrorType = ErrorType.unsupported_shipping_address;
            result.ErrorText = "Blaaat";

            return null;
        }

        public override IPurchaseOrder GetPurchaseOrderByKlarnaOrderId(string orderId)
        {
            OrderSearchOptions searchOptions = new OrderSearchOptions();
            searchOptions.CacheResults = false;
            searchOptions.StartingRecord = 0;
            searchOptions.RecordsToRetrieve = 1;
            searchOptions.Classes = new System.Collections.Specialized.StringCollection { "PurchaseOrder" };
            searchOptions.Namespace = "Mediachase.Commerce.Orders";

            var parameters = new OrderSearchParameters();
            parameters.SqlMetaWhereClause = $"META.{Constants.KlarnaCheckoutOrderIdField} LIKE '{orderId}'";

            var purchaseOrder = OrderContext.Current.FindPurchaseOrders(parameters, searchOptions)?.FirstOrDefault();

            if (purchaseOrder != null)
            {
                return _orderRepository.Load<IPurchaseOrder>(purchaseOrder.OrderGroupId);
            }
            return null;
        }

        private IEnumerable<ShippingOption> GetShippingOptions(ICart cart)
        {
            var methods = ShippingManager.GetShippingMethodsByMarket(cart.Market.MarketId.Value, false);
            return methods.ShippingMethod.Select(method => new ShippingOption
            {
                Id = method.ShippingMethodId.ToString(),
                Name = method.DisplayName,
                Price = AmountHelper.GetAmount(method.BasePrice),
                PreSelected = method.IsDefault,
                TaxAmount = 0,
                TaxRate = 0,
                Description = method.Description
            });
        }

        private List<OrderLine> GetOrderLines(ICart cart)
        {
            var market = cart.Market;
            var shipment = cart.GetFirstShipment();
            var orderLines = new List<OrderLine>();

            // Line items
            foreach (var lineItem in cart.GetAllLineItems())
            {
                var orderLine = lineItem.GetOrderLine();
                orderLines.Add(orderLine);
            }

            // Shipment
            var totals = _orderGroupTotalsCalculator.GetTotals(cart);
            if (shipment != null && totals.ShippingTotal.Amount > 0)
            {
                var shipmentOrderLine = shipment.GetOrderLine(totals);
                orderLines.Add(shipmentOrderLine);
            }

            // Sales tax
            orderLines.Add(new PatchedOrderLine()
            {
                Type = "sales_tax",
                Name = "Sales Tax",
                Quantity = 1,
                TotalAmount = AmountHelper.GetAmount(totals.TaxTotal),
                UnitPrice = AmountHelper.GetAmount(totals.TaxTotal),
                TotalTaxAmount = 0,
                TaxRate = 0
            });

            // Order level discounts
            var orderDiscount = cart.GetOrderDiscountTotal(cart.Currency);
            var entryLevelDiscount = cart.GetAllLineItems().Sum(x => x.GetEntryDiscount());
            var totalDiscount = orderDiscount.Amount + entryLevelDiscount;
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

            return orderLines;
        }

        private MerchantUrls GetMerchantUrls(ICart cart)
        {
            var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaCheckoutSystemKeyword, ContentLanguage.PreferredCulture.Name);
            if (paymentMethod != null)
            {
                return new MerchantUrls
                {
                    Terms = new Uri(paymentMethod.GetParameter(Constants.TermsUrlField)),
                    Checkout = new Uri(paymentMethod.GetParameter(Constants.CheckoutUrlField)),
                    Confirmation = new Uri(paymentMethod.GetParameter(Constants.ConfirmationUrlField).Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString())),
                    Push = new Uri(paymentMethod.GetParameter(Constants.PushUrlField)),
                    AddressUpdate = new Uri(paymentMethod.GetParameter(Constants.AddressUpdateUrlField).Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString())),
                    ShippingOptionUpdate = new Uri(paymentMethod.GetParameter(Constants.ShippingOptionUpdateUrlField).Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString())),
                    Notification = new Uri(paymentMethod.GetParameter(Constants.NotificationUrlField)),
                    Validation = new Uri(paymentMethod.GetParameter(Constants.OrderValidationUrlField).Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString()))
                };
            }
            return null;
        }

        private IEnumerable<string> GetCountries()
        {
            var countries = CountryManager.GetCountries();

            return CountryCodeHelper.GetTwoLetterCountryCodes(countries.Country.Select(x => x.Code));
        }
    }
}