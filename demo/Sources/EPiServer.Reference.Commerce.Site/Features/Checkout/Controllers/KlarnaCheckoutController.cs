using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using Klarna.Checkout;
using Klarna.Checkout.Models;
using Klarna.Common.Models;
using Mediachase.Commerce.Orders;
using Newtonsoft.Json;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    [RoutePrefix("klarnacheckout")]
    public class KlarnaCheckoutController : ApiController
    {
        private ILogger _log = LogManager.GetLogger(typeof(KlarnaPaymentController));
        private readonly IKlarnaCheckoutService _klarnaCheckoutService;
        private readonly IOrderRepository _orderRepository;
        private readonly ILineItemValidator _lineItemValidator;
        private readonly ICartService _cartService;
        private readonly CheckoutService _checkoutService;


        public KlarnaCheckoutController(
            IKlarnaCheckoutService klarnaCheckoutService,
            IOrderRepository orderRepository, 
            ILineItemValidator lineItemValidator, 
            ICartService cartService, 
            CheckoutService checkoutService)
        {
            _klarnaCheckoutService = klarnaCheckoutService;
            _orderRepository = orderRepository;
            _lineItemValidator = lineItemValidator;
            _cartService = cartService;
            _checkoutService = checkoutService;
        }
        
        [Route("cart/{orderGroupId}/shippingoptionupdate")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public IHttpActionResult ShippingOptionUpdate(int orderGroupId, [FromBody]ShippingOptionUpdateRequest shippingOptionUpdateRequest)
        {
            var cart = _orderRepository.Load<ICart>(orderGroupId);

            var response = _klarnaCheckoutService.UpdateShippingMethod(cart, shippingOptionUpdateRequest);

            return Ok(response);
        }

        [Route("cart/{orderGroupId}/addressupdate")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public IHttpActionResult AddressUpdate(int orderGroupId, [FromBody]AddressUpdateRequest addressUpdateRequest)
        {
            var cart = _orderRepository.Load<ICart>(orderGroupId);

            var response = _klarnaCheckoutService.UpdateAddress(cart, addressUpdateRequest);

            return Ok(response);
        }

        [Route("cart/{orderGroupId}/ordervalidation")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public IHttpActionResult OrderValidation(int orderGroupId, [FromBody]PatchedCheckoutOrderData checkoutData)
        {
            var cart = _orderRepository.Load<ICart>(orderGroupId);

            // Validate order amount, shipping address
            if (!_klarnaCheckoutService.ValidateOrder(cart, checkoutData))
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.RedirectMethod);
                httpResponseMessage.Headers.Location = new Uri("http://klarna.localtest.me?redirect");
                return ResponseMessage(httpResponseMessage);
            }

            // Validate cart lineitems
            var validationIssues = new Dictionary<ILineItem, ValidationIssue>();
            cart.ValidateOrRemoveLineItems((lineItem, validationIssue) =>
            {
                validationIssues.Add(lineItem, validationIssue);
            }, _lineItemValidator);

            if (validationIssues.Any())
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.RedirectMethod);
                httpResponseMessage.Headers.Location = new Uri("http://klarna.localtest.me?redirect");
                return ResponseMessage(httpResponseMessage);
            }

            // To return an error like this you need require_validate_callback_success set to true
            if (checkoutData.ShippingAddress.PostalCode.Equals("94108-2704"))
            {
                var errorResult = new ErrorResult()
                {
                    ErrorType = ErrorType.address_error,
                    ErrorText = "Can't ship to postalcode 94108-2704"
                };
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, errorResult));
            }

            // Order is valid, create on hold cart in epi
            cart.Name = OrderStatus.OnHold.ToString();
            _orderRepository.Save(cart);

            // Create new default cart
            var newCart = _orderRepository.Create<ICart>(cart.CustomerId, _cartService.DefaultCartName);
            _orderRepository.Save(newCart);

            return Ok();
        }

        [Route("cart/{orderGroupId}/fraud")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public IHttpActionResult FraudNotification(int orderGroupId, string klarna_order_id)
        {
            var purchaseOrder = GetOrCreatePurchaseOrder(orderGroupId, klarna_order_id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }

            var requestParams = Request.Content.ReadAsStringAsync().Result;

            if (!string.IsNullOrEmpty(requestParams))
            {
                var notification = JsonConvert.DeserializeObject<NotificationModel>(requestParams);

                _klarnaCheckoutService.FraudUpdate(notification);

            }
            return Ok();
        }

        [Route("cart/{orderGroupId}/push")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public IHttpActionResult Push(int orderGroupId, string klarna_order_id)
        {
            if (klarna_order_id == null)
            {
                return BadRequest();
            }
            var purchaseOrder = GetOrCreatePurchaseOrder(orderGroupId, klarna_order_id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }

            // Update merchant reference
            _klarnaCheckoutService.UpdateMerchantReference1(purchaseOrder);

            // Acknowledge the order through the order management API
            _klarnaCheckoutService.AcknowledgeOrder(purchaseOrder);

            return Ok();
        }

        private IPurchaseOrder GetOrCreatePurchaseOrder(int orderGroupId, string klarnaOrderId)
        {
            // Check if the order has been created already
            var purchaseOrder = _klarnaCheckoutService.GetPurchaseOrderByKlarnaOrderId(klarnaOrderId);
            if (purchaseOrder != null)
            {
                return purchaseOrder;
            }
            
            // Check if we still have a cart and can create an order
            var cart = _orderRepository.Load<ICart>(orderGroupId);
            var cartKlarnaOrderId = cart.Properties[Constants.KlarnaCheckoutOrderIdField]?.ToString();
            if (cartKlarnaOrderId == null || !cartKlarnaOrderId.Equals(klarnaOrderId))
            {
                return null;
            }

            var order = _klarnaCheckoutService.GetOrder(klarnaOrderId);
            if (!order.Status.Equals("checkout_complete"))
            {
                // Won't create order, Klarna checkout not complete
                return null;
            }
            purchaseOrder = _checkoutService.CreatePurchaseOrderForKlarna(klarnaOrderId, order, cart);
            return purchaseOrder;
        }
    }
}