using System;
using System.Linq;
using EPiServer.Commerce.Order;
using Foundation.Features.Checkout.Services;
using Foundation.Features.MyAccount.AddressBook;
using Klarna.Common.Extensions;
using Klarna.Common.Models;
using Klarna.Payments;
using Klarna.Payments.Models;
using Mediachase.Commerce.Customers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Features.Api
{
    /// <summary>
    /// Klarna Payments example API for callbacks
    /// </summary>
    [Route("klarnaapi")]
    public class KlarnaPaymentsApiController : Controller
    {
        private readonly CustomerContext _customerContext;
        private readonly IKlarnaPaymentsService _klarnaPaymentsService;
        private readonly ICartService _cartService;
        private readonly CheckoutService _checkoutService;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IAddressBookService _addressBookService;
        private readonly IOrderRepository _orderRepository;

        public KlarnaPaymentsApiController(
            CustomerContext customerContext,
            IKlarnaPaymentsService klarnaPaymentsService,
            ICartService cartService,
            CheckoutService checkoutService,
            IPurchaseOrderRepository purchaseOrderRepository,
            IAddressBookService addressBookService,
            IOrderRepository orderRepository)
        {
            _klarnaPaymentsService = klarnaPaymentsService;
            _customerContext = customerContext;
            _cartService = cartService;
            _checkoutService = checkoutService;
            _purchaseOrderRepository = purchaseOrderRepository;
            _addressBookService = addressBookService;
            _orderRepository = orderRepository;
        }

        [Route("personal")]
        [HttpPost]
        public ActionResult GetPersonalInformation(string billingAddressId)
        {
            if (string.IsNullOrWhiteSpace(billingAddressId))
            {
                return BadRequest();
            }

            var cart = _cartService.LoadCart(_cartService.DefaultCartName, false);
            var sessionRequest = GetPersonalInformationSession(cart.Cart, billingAddressId);

            sessionRequest.ShippingAddress = sessionRequest.BillingAddress;

            return Ok(sessionRequest);
        }

        [Route("order/confirmation")]
        [HttpGet]
        public ActionResult OrderConfirmation(string orderNumber)
        {
            var purchaseOrder = _purchaseOrderRepository.Load(orderNumber);

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            return Redirect(_checkoutService.BuildRedirectionUrl(null, purchaseOrder, true));
        }

        [Route("personal/allow")]
        [HttpPost]
        public ActionResult AllowSharingOfPersonalInformation()
        {
            var cart = _cartService.LoadCart(_cartService.DefaultCartName, false);
            if (_klarnaPaymentsService.AllowSharingOfPersonalInformation(cart.Cart))
            {
                return Ok();
            }

            return StatusCode(StatusCodes.Status500InternalServerError, "");
        }

        [Route("express/authenticated")]
        [HttpPost]
        public ActionResult ExpressButtonAuthenticated([FromBody] OrderManagementAddressInfo addressInfo)
        {
            if (addressInfo == null || string.IsNullOrEmpty(addressInfo.GivenName) || string.IsNullOrEmpty(addressInfo.FamilyName))
            {
                return BadRequest();
            }

            var cart = _cartService.LoadCart(_cartService.DefaultCartName, false);
            var shipment = cart.Cart.GetFirstShipment();
            var payment = cart.Cart.GetFirstForm()?.Payments.FirstOrDefault();

            var addressModel = new AddressModel
            {
                Name = $"{addressInfo.GivenName} {addressInfo.FamilyName} {DateTime.Now}",
                FirstName = addressInfo.GivenName,
                LastName = addressInfo.FamilyName,
                Line1 = addressInfo.StreetAddress,
                Line2 = addressInfo.StreetAddress2,
                City = addressInfo.City,
                PostalCode = addressInfo.PostalCode,
                CountryRegion = new CountryRegionViewModel {Region = addressInfo.Region},
                CountryCode = addressInfo.Country,
                Email = addressInfo.Email,
                DaytimePhoneNumber = addressInfo.Phone
            };

            var orderAddress = _addressBookService.ConvertToAddress(addressModel, cart.Cart);

            if (shipment != null)
            {
                shipment.ShippingAddress = orderAddress;
            }

            if (payment != null)
            {
                payment.BillingAddress = orderAddress;
            }

            _orderRepository.Save(cart.Cart);

            return Ok();
        }

        [Route("fraud")]
        [HttpPost]
        public ActionResult FraudNotification(NotificationModel notification)
        {
            _klarnaPaymentsService.FraudUpdate(notification);
            return Ok();
        }

        private PersonalInformationSession GetPersonalInformationSession(ICart cart, string billingAddressId)
        {
            var request = _klarnaPaymentsService.GetPersonalInformationSession(cart);

            // Get billing address info
            var billingAddress = _customerContext.CurrentContact?.ContactAddresses.FirstOrDefault(x => x.Name == billingAddressId)?.ToOrderAddress();

            request.BillingAddress = billingAddress;

            return request;
        }
    }
}