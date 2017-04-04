using System.Threading.Tasks;
using System.Web.Http;
using EPiServer.ServiceLocation;
using Klarna.Payments;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    [RoutePrefix("klarnaapi")]
    public class KlarnaPaymentController : ApiController
    {
        [Route("session/{sessionId}")]
        [AcceptVerbs("GET")]
        public async Task<IHttpActionResult> GetSession(string sessionId)
        {
            var klarnaService = ServiceLocator.Current.GetInstance<IKlarnaService>();

            var result = await klarnaService.GetSession(sessionId);

            return Ok(result);
        }
    }
}