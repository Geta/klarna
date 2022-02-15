using System.Linq;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Foundation.Features.Checkout.Services;
using Klarna.Checkout;
using Klarna.Checkout.Models;
using Klarna.Common.Models;
using Mediachase.Commerce.Markets;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Features.Checkout
{
    [Route("klarnacheckout")]
    public class KlarnaCheckoutController : Controller
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
        public ActionResult ShippingOptionUpdate(int orderGroupId, [FromBody] ShippingOptionUpdateRequest shippingOptionUpdateRequest)
        {
            var cart = _orderRepository.Load<ICart>(orderGroupId);

            var response = _klarnaCheckoutService.UpdateShippingMethod(cart, shippingOptionUpdateRequest);

            return Ok(response);
        }

        [Route("cart/{orderGroupId}/addressupdate")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public ActionResult AddressUpdate(int orderGroupId, [FromBody] CallbackAddressUpdateRequest addressUpdateRequest)
        {
            var cart = _orderRepository.Load<ICart>(orderGroupId);
            var response = _klarnaCheckoutService.UpdateAddress(cart, addressUpdateRequest);
            return Ok(response);
        }

        [Route("cart/{orderGroupId}/ordervalidation")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public ActionResult OrderValidation(int orderGroupId, [FromBody] CheckoutOrder checkoutData)
        {
            // More information: https://docs.klarna.com/klarna-checkout/popular-use-cases/validate-order/

            var cart = _orderRepository.Load<ICart>(orderGroupId);

            // Validate cart lineitems
            var validationIssues = _cartService.ValidateCart(cart);
            if (validationIssues.Any())
            {
                // check validation issues and redirect to a page to display the error
                return Redirect("/en/error-pages/checkout-something-went-wrong/");
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
                return BadRequest(errorResult);
            }

            // Validate order amount, shipping address
            if (!_klarnaCheckoutService.ValidateOrder(cart, checkoutData))
            {
                return Redirect("/en/error-pages/checkout-something-went-wrong/");
            }

            return Ok();
        }

        [Route("fraud")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public ActionResult FraudNotification(NotificationModel notification)
        {
            _klarnaCheckoutService.FraudUpdate(notification);
            return Ok();
        }

        [Route("cart/{orderGroupId}/push")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public async Task<ActionResult> Push(int orderGroupId, string klarna_order_id)
        {
            if (klarna_order_id == null)
            {
                return BadRequest();
            }
            var purchaseOrder = await GetOrCreatePurchaseOrder(orderGroupId, klarna_order_id).ConfigureAwait(false);
            if (purchaseOrder == null)
            {
                return NotFound();
            }

            // Update merchant reference
            await _klarnaCheckoutService.UpdateMerchantReference1(purchaseOrder).ConfigureAwait(false);

            // Acknowledge the order through the order management API
            _klarnaCheckoutService.AcknowledgeOrder(purchaseOrder);

            return Ok();
        }

        private async Task<IPurchaseOrder> GetOrCreatePurchaseOrder(int orderGroupId, string klarnaOrderId)
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
            var order = await _klarnaCheckoutService.GetOrder(klarnaOrderId, market).ConfigureAwait(false);
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