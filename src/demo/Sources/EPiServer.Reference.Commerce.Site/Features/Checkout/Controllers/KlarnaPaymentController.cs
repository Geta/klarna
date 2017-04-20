using System.Threading.Tasks;
using System.Web.Http;
using Klarna.Payments;
using EPiServer.Logging;
using Klarna.Payments.Models;
using Newtonsoft.Json;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    [RoutePrefix("klarnaapi")]
    public class KlarnaPaymentController : ApiController
    {
        private ILogger _log = LogManager.GetLogger(typeof(KlarnaPaymentController));
        private readonly IKlarnaService _klarnaService;

        public KlarnaPaymentController(IKlarnaService klarnaService)
        {
            _klarnaService = klarnaService;
        }

        [Route("session/{sessionId}")]
        [AcceptVerbs("GET")]
        public async Task<IHttpActionResult> GetSession(string sessionId)
        {
            var result = await _klarnaService.GetSession(sessionId);

            return Ok(result);
        }

        [Route("fraud/")]
        [AcceptVerbs("Post")]
        [HttpPost]
        public IHttpActionResult FraudNotification()
        {
            var requestParams = Request.Content.ReadAsStringAsync().Result;

            _log.Error("KlarnaPaymentController.FraudNotification called: " + requestParams);

            if (!string.IsNullOrEmpty(requestParams))
            {
                var notification = JsonConvert.DeserializeObject<NotificationModel>(requestParams);

                _klarnaService.FraudUpdate(notification);
            }
            return Ok();
        }
    }
}