using System.Web.Http;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Klarna.Checkout;
using Klarna.Checkout.Models;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    [RoutePrefix("klarnacheckout")]
    public class KlarnaCheckoutController : ApiController
    {
        private ILogger _log = LogManager.GetLogger(typeof(KlarnaPaymentController));
        private readonly CustomerContextFacade _customerContextFacade;
        private readonly IKlarnaCheckoutService _klarnaCheckoutService;
        private readonly ICartService _cartService;
        private readonly IOrderRepository _orderRepository;

        public KlarnaCheckoutController(
            CustomerContextFacade customerContextFacade,
            IKlarnaCheckoutService klarnaCheckoutService, 
            ICartService cartService,
            IOrderRepository orderRepository)
        {
            _klarnaCheckoutService = klarnaCheckoutService;
            _customerContextFacade = customerContextFacade;
            _cartService = cartService;
            _orderRepository = orderRepository;
        }

        [Route("cart/{orderGroupId}/shippingoptionupdate")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public IHttpActionResult ShippingOptionUpdate(int orderGroupId, [FromBody]PatchedCheckoutOrderData checkoutData)
        {
            var cart = _orderRepository.Load<ICart>(orderGroupId);

            _klarnaCheckoutService.UpdateShippingMethod(cart, checkoutData);

            return Ok(checkoutData);
        }

        [Route("cart/{orderGroupId}/addressupdate")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public IHttpActionResult AddressUpdate(int orderGroupId, [FromBody]PatchedCheckoutOrderData checkoutData)
        {
            var cart = _orderRepository.Load<ICart>(orderGroupId);

            _klarnaCheckoutService.UpdateAddress(cart, checkoutData);

            return Ok(checkoutData);
        }

        [Route("cart/{orderGroupId}/ordervalidation")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public IHttpActionResult OrderValidation(int orderGroupId, [FromBody]PatchedCheckoutOrderData checkoutData)
        {
            var cart = _orderRepository.Load<ICart>(orderGroupId);

            // TODO validate order
            return Ok();
        }
    }
}