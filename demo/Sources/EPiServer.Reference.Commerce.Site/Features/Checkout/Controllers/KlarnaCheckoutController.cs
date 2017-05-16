using System.Net;
using System.Net.Http;
using System.Web.Http;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Klarna.Checkout;
using Klarna.Checkout.Models;
using Klarna.Common.Models;
using Newtonsoft.Json;

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
        public HttpResponseMessage OrderValidation(int orderGroupId, [FromBody]PatchedCheckoutOrderData checkoutData)
        {
            var cart = _orderRepository.Load<ICart>(orderGroupId);

            //var newCart = _orderRepository.Create<ICart>(cart.CustomerId, cart.Name);
            //newCart.CopyFrom(cart, null);

            var errorResult = _klarnaCheckoutService.ValidateOrder(cart, checkoutData);
            if (errorResult == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, errorResult);
        }

        [Route("fraud/")]
        [AcceptVerbs("Post")]
        [HttpPost]
        public IHttpActionResult FraudNotification()
        {
            var requestParams = Request.Content.ReadAsStringAsync().Result;

            _log.Error("KlarnaCheckoutController.FraudNotification called: " + requestParams);

            if (!string.IsNullOrEmpty(requestParams))
            {
                var notification = JsonConvert.DeserializeObject<NotificationModel>(requestParams);

                _klarnaCheckoutService.FraudUpdate(notification);

            }
            return Ok();
        }

        [Route("push/")]
        [AcceptVerbs("Post")]
        [HttpPost]
        public IHttpActionResult PushNotification()
        {
            var requestParams = Request.Content.ReadAsStringAsync().Result;

            _log.Error("KlarnaCheckoutController.PushNotification called: " + requestParams);

            if (!string.IsNullOrEmpty(requestParams))
            {

            }
            return Ok(requestParams);
        }
    }
}