using System.Web.Http;
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

        public KlarnaCheckoutController(
            CustomerContextFacade customerContextFacade,
            IKlarnaCheckoutService klarnaCheckoutService, 
            ICartService cartService)
        {
            _klarnaCheckoutService = klarnaCheckoutService;
            _customerContextFacade = customerContextFacade;
            _cartService = cartService;
        }

        [Route("shippingoptionupdate")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public IHttpActionResult ShippingOptionUpdate([FromBody]PatchedCheckoutOrderData checkoutData)
        {
            // TODO update cart
            return Ok(checkoutData);
        }

        [Route("addressupdate")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public IHttpActionResult AddressUpdate([FromBody]PatchedCheckoutOrderData checkoutData)
        {
            // TODO update cart
            return Ok(checkoutData);
        }

        [Route("ordervalidation")]
        [AcceptVerbs("POST")]
        [HttpPost]
        public IHttpActionResult OrderValidation([FromBody]PatchedCheckoutOrderData checkoutData)
        {
            // TODO validate order
            return Ok();
        }
    }
}