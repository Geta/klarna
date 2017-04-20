using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.ServiceLocation;
using Klarna.Payments;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Klarna.Payments.Extensions;
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

        [Route("address/{addressId}")]
        [AcceptVerbs("GET")]
        public IHttpActionResult GetAddress(string addressId)
        {
            var customerContextFacade = ServiceLocator.Current.GetInstance<CustomerContextFacade>();
            var address = customerContextFacade.CurrentContact.ContactAddresses.FirstOrDefault(x => x.Name == addressId)?.ToAddress();
            return Ok(address);
        }

        [Route("personal")]
        [AcceptVerbs("GET")]
        public IHttpActionResult GetpersonalInformation()
        {
            var klarnaService = ServiceLocator.Current.GetInstance<IKlarnaService>();
            var cartService = ServiceLocator.Current.GetInstance<ICartService>();
            var cart = cartService.LoadCart(cartService.DefaultCartName);
            var sessionRequest = klarnaService.GetSessionRequest(cart).ToPersonalInformationSession();

            /*if (klarnaService.Configuration.PreAssesmentENabnled)
            {
                sessionReuest.
            }*/

            return Ok(sessionRequest);
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