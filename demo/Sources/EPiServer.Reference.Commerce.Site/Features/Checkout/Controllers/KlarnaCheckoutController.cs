using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using Klarna.Checkout;
using Klarna.Checkout.Models;
using Klarna.Common.Models;
using Klarna.Rest.Core.Model;
using Mediachase.Commerce.Markets;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    [RoutePrefix("klarnacheckout")]
    public class KlarnaCheckoutController : ApiController
    {
        private readonly IKlarnaCheckoutService _klarnaCheckoutService;
        private readonly IOrderRepository _orderRepository;
        private readonly ICartService _cartService;
        private readonly CheckoutService _checkoutService;
        private readonly IMarketService _marketService;


        public KlarnaCheckoutController(
            IKlarnaCheckoutService klarnaCheckoutService,
            IOrderRepository orderRepository,
            ICartService cartService,
            CheckoutService checkoutService,
            IMarketService marketService)
        {
            _klarnaCheckoutService = klarnaCheckoutService;
            _orderRepository = orderRepository;
            _cartService = cartService;
            _checkoutService = checkoutService;
            _marketService = marketService;
        }

        [Route("cart/{orderGroupId}/shippingoptionupdate")]
        [AcceptVerbs("POST")]
        [HttpPost]
        [ResponseType(typeof(ShippingOptionUpdateResponse))]
        public IHttpActionResult ShippingOptionUpdate(int orderGroupId, [FromBody]ShippingOptionUpdateRequest shippingOptionUpdateRequest)
        {
            var cart = _orderRepository.Load<ICart>(orderGroupId);

            var response = _klarnaCheckoutService.UpdateShippingMethod(cart, shippingOptionUpdateRequest);

            return Ok(response);
        }

        [Route("cart/{orderGroupId}/addressupdate")]
        [AcceptVerbs("POST")]
        [HttpPost]
        [ResponseType(typeof(AddressUpdateResponse))]
        public IHttpActionResult AddressUpdate(int orderGroupId, [FromBody]CallbackAddressUpdateRequest addressUpdateRequest)
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

            // Validate cart lineitems
            var validationIssues = _cartService.ValidateCart(cart);
            if (validationIssues.Any())
            {
                // check validation issues and redirect to a page to display the error
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.RedirectMethod);
                httpResponseMessage.Headers.Location = new Uri("http://klarna.geta.no/en/error-pages/checkout-something-went-wrong/");
                return ResponseMessage(httpResponseMessage);
            }

            // Validate billing address if necessary (this is just an example)
            // To return an error like this you need require_validate_callback_success set to true
            if (checkoutData.BillingCheckoutAddress.PostalCode.Equals("94108-2704"))
            {
                var errorResult = new ErrorResult
                {
                    ErrorType = ErrorType.address_error,
                    ErrorText = "Can't ship to postalcode 94108-2704"
                };
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, errorResult));
            }

            // Validate order amount, shipping address
            if (!_klarnaCheckoutService.ValidateOrder(cart, checkoutData))
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.RedirectMethod);
                httpResponseMessage.Headers.Location = new Uri("http://klarna.geta.no/en/error-pages/checkout-something-went-wrong/");
                return ResponseMessage(httpResponseMessage);
            }

            return Ok();
        }

        [Route("fraud")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public IHttpActionResult FraudNotification(NotificationModel notification)
        {
            _klarnaCheckoutService.FraudUpdate(notification);
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
            var cartKlarnaOrderId = cart.Properties[Constants.KlarnaCheckoutOrderIdCartField]?.ToString();
            if (cartKlarnaOrderId == null || !cartKlarnaOrderId.Equals(klarnaOrderId))
            {
                return null;
            }

            var market = _marketService.GetMarket(cart.MarketId);
            var order = _klarnaCheckoutService.GetOrder(klarnaOrderId, market);
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