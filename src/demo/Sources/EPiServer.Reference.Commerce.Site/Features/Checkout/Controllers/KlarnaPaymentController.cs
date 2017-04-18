using System.Threading.Tasks;
using System.Web.Http;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.ServiceLocation;
using Klarna.Payments;
using EPiServer.Logging;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    [RoutePrefix("klarnaapi")]
    public class KlarnaPaymentController : ApiController
    {
        private ILogger _log = LogManager.GetLogger(typeof(KlarnaPaymentController));

        [Route("session/{sessionId}")]
        [AcceptVerbs("GET")]
        public async Task<IHttpActionResult> GetSession(string sessionId)
        {
            var klarnaService = ServiceLocator.Current.GetInstance<IKlarnaService>();

            var result = await klarnaService.GetSession(sessionId);

            return Ok(result);
        }

        [Route("authorization")]
        [AcceptVerbs("GET")]
        public async Task<IHttpActionResult> GetAuthorization()
        {
            var klarnaService = ServiceLocator.Current.GetInstance<IKlarnaService>();
            var cartService = ServiceLocator.Current.GetInstance<ICartService>();

            var cart = cartService.LoadCart(cartService.DefaultCartName);
            
            var result = await klarnaService.GetAuthorizationModel(klarnaService.GetSessionId(cart));

            return Ok(result);
        }

        [Route("fraud/")]
        [AcceptVerbs("Post")]
        [HttpPost]
        public IHttpActionResult FraudNotification()
        {
            var requestParams = Request.Content.ReadAsStringAsync().Result;

            _log.Error("KlarnaPaymentController.FraudNotification called: " + requestParams);
            var klarnaService = ServiceLocator.Current.GetInstance<IKlarnaService>();

            klarnaService.FraudUpdate(null);

            return Ok();
        }
    }
}