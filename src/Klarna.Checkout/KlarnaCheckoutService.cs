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
using Klarna.OrderManagement;
using Klarna.Rest;
using Klarna.Rest.Models;
using Klarna.Rest.Transport;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace Klarna.Checkout
{
    [ServiceConfiguration(typeof(IKlarnaCheckoutService))]
    public class KlarnaCheckoutService : KlarnaService, IKlarnaCheckoutService
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(KlarnaCheckoutService));
        private readonly IOrderGroupTotalsCalculator _orderGroupTotalsCalculator;

        private readonly IKlarnaOrderValidator _klarnaOrderValidator;
        private readonly IOrderRepository _orderRepository;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IKlarnaOrderService _klarnaOrderService;

        private Client _client;
        private PaymentMethodDto _paymentMethodDto;
        
        public KlarnaCheckoutService(
            IOrderGroupTotalsCalculator orderGroupTotalsCalculator,
            IOrderRepository orderRepository,
            IConnectionFactory connectionFactory,
            IPaymentProcessor paymentProcessor,
            IOrderGroupCalculator orderGroupCalculator,
            IKlarnaOrderValidator klarnaOrderValidator) : base(orderRepository, paymentProcessor, orderGroupCalculator)
        {
            _orderGroupTotalsCalculator = orderGroupTotalsCalculator;
            _orderRepository = orderRepository;
            _connectionFactory = connectionFactory;
            _klarnaOrderValidator = klarnaOrderValidator;
            _klarnaOrderService = new KlarnaOrderService(_connectionFactory.GetConnectionConfiguration(PaymentMethodDto));
        }

        public PaymentMethodDto PaymentMethodDto
        {
            get
            {
                if (_paymentMethodDto == null)
                {
                    _paymentMethodDto = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaCheckoutSystemKeyword, ContentLanguage.PreferredCulture.Name);
                }
                return _paymentMethodDto;
            }
        }


        public Client Client
        {
            get
            {
                if (_client == null)
                {
                    if (PaymentMethodDto != null)
                    {
                        var connectionConfiguration = _connectionFactory.GetConnectionConfiguration(PaymentMethodDto);
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

            // KCO_4: In case of signed in user the email address and default address details will be prepopulated by data from Merchant system.
            var customerContact = CustomerContext.Current.GetContactById(cart.CustomerId);
            if (customerContact?.PreferredBillingAddress != null)
            {
                orderData.BillingAddress = customerContact.PreferredBillingAddress.ToAddress();
            }

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
                _logger.Error($"{ex.ErrorMessage.CorrelationId} {ex.ErrorMessage.ErrorCode} {string.Join(", ", ex.ErrorMessage.ErrorMessages)}", ex);
            }
            catch (WebException ex)
            {
                _logger.Error(ex.Message, ex);
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
                _logger.Error($"{ex.ErrorMessage.CorrelationId} {ex.ErrorMessage.ErrorCode} {string.Join(", ", ex.ErrorMessage.ErrorMessages)}", ex);
            }
            catch (WebException ex)
            {
                _logger.Error(ex.Message, ex);
            }
            return null;
        }

        private CheckoutOrderData GetCheckoutOrderData(ICart cart)
        {
            var totals = _orderGroupTotalsCalculator.GetTotals(cart);
            var shipment = cart.GetFirstShipment();
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
                OrderLines = GetOrderLines(cart, totals),
                ShippingAddress = shipment?.ShippingAddress?.ToAddress()
            } as CheckoutOrderData;
            
            if (PaymentMethodDto != null)
            {
                orderData.Options = GetOptions(PaymentMethodDto);
            }
            return orderData;
        }

        private CheckoutOptions GetOptions(PaymentMethodDto paymentMethod)
        {
            var options = new PatchedCheckoutOptions
            {
                RequireValidateCallbackSuccess = bool.Parse(paymentMethod.GetParameter(Constants.RequireValidateCallbackSuccessField, "false")),
                AllowSeparateShippingAddress = bool.Parse(paymentMethod.GetParameter(Constants.AllowSeparateShippingAddressField, "false")),
                ColorButton = paymentMethod.GetParameter(Constants.KlarnaWidgetColorButtonField, "#FF9900"),
                ColorButtonText = paymentMethod.GetParameter(Constants.KlarnaWidgetColorButtonTextField, "#FF9900"),
                ColorCheckbox = paymentMethod.GetParameter(Constants.KlarnaWidgetColorCheckboxField, "#FF9900"),
                ColorCheckboxCheckmark = paymentMethod.GetParameter(Constants.KlarnaWidgetColorCheckboxCheckmarkField, "#FF9900"),
                ColorHeader = paymentMethod.GetParameter(Constants.KlarnaWidgetColorHeaderField, "#FF9900"),
                ColorLink = paymentMethod.GetParameter(Constants.KlarnaWidgetColorLinkField, "#FF9900"),
                DateOfBirthMandatory = bool.Parse(paymentMethod.GetParameter(Constants.DateOfBirthMandatoryField, "false")),
                ShowSubtotalDetail = bool.Parse(paymentMethod.GetParameter(Constants.ShowSubtotalDetailField, "false")),
                TitleMandatory = bool.Parse(paymentMethod.GetParameter(Constants.TitleMandatoryField, "false")),
                RadiusBorder = paymentMethod.GetParameter(Constants.KlarnaWidgetRadiusBorderField, "5px"),
                ShippingDetails = paymentMethod.GetParameter(Constants.ShippingDetailsField, string.Empty),
            };

            var additionalCheckboxText = paymentMethod.GetParameter(Constants.AdditionalCheckboxTextField, string.Empty);
            if (!string.IsNullOrEmpty(additionalCheckboxText))
            {
                options.AdditionalCheckbox = new AdditionalCheckbox
                {
                    Text = additionalCheckboxText,
                    Checked = bool.Parse(paymentMethod.GetParameter(Constants.AdditionalCheckboxDefaultCheckedField, "false")),
                    Required = bool.Parse(paymentMethod.GetParameter(Constants.AdditionalCheckboxRequiredField, "false")),
                };
            }
            return options;
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
                OrderLines = GetOrderLines(cart, totals),
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
                OrderLines = GetOrderLines(cart, totals),
                PurchaseCurrency = cart.Currency.CurrencyCode,
                ShippingOptions = GetShippingOptions(cart)
            };
        }

        public bool ValidateOrder(ICart cart, PatchedCheckoutOrderData checkoutData)
        {
            // Compare the current cart state to the Klarna order state (totals, shipping selection, discounts, and gift cards). If they don't match there is an issue.
            var localCheckoutOrderData = GetCheckoutOrderData(cart) as PatchedCheckoutOrderData;
            return _klarnaOrderValidator.Compare(checkoutData, localCheckoutOrderData);
        }

        public void CancelOrder(ICart cart)
        {
            var orderId = cart.Properties[Constants.KlarnaCheckoutOrderIdField]?.ToString();
            if (!string.IsNullOrWhiteSpace(orderId))
            {
                _klarnaOrderService.CancelOrder(orderId);

                cart.Properties[Constants.KlarnaCheckoutOrderIdField] = null;
                _orderRepository.Save(cart);
            }
        }

        public void UpdateMerchantReference1(IPurchaseOrder purchaseOrder)
        {
            var orderId = purchaseOrder.Properties[Constants.KlarnaCheckoutOrderIdField]?.ToString();
            if (!string.IsNullOrWhiteSpace(orderId) && purchaseOrder is PurchaseOrder)
            {
                _klarnaOrderService.UpdateMerchantReferences(orderId, ((PurchaseOrder)purchaseOrder).TrackingNumber, null);
            }
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

        private MerchantUrls GetMerchantUrls(ICart cart)
        {
            if (PaymentMethodDto != null)
            {
                return new MerchantUrls
                {
                    Terms = new Uri(PaymentMethodDto.GetParameter(Constants.TermsUrlField)),
                    Checkout = new Uri(PaymentMethodDto.GetParameter(Constants.CheckoutUrlField)),
                    Confirmation = new Uri(PaymentMethodDto.GetParameter(Constants.ConfirmationUrlField).Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString())),
                    Push = new Uri(PaymentMethodDto.GetParameter(Constants.PushUrlField).Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString())),
                    AddressUpdate = new Uri(PaymentMethodDto.GetParameter(Constants.AddressUpdateUrlField).Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString())),
                    ShippingOptionUpdate = new Uri(PaymentMethodDto.GetParameter(Constants.ShippingOptionUpdateUrlField).Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString())),
                    Notification = new Uri(PaymentMethodDto.GetParameter(Constants.NotificationUrlField)),
                    Validation = new Uri(PaymentMethodDto.GetParameter(Constants.OrderValidationUrlField).Replace("{orderGroupId}", cart.OrderLink.OrderGroupId.ToString()))
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