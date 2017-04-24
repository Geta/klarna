using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using EPiServer.Commerce.Order;
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
        private readonly CustomerContextFacade _customerContextFacade;
        private readonly IKlarnaService _klarnaService;
        private readonly ICartService _cartService;

        public KlarnaPaymentController(
            CustomerContextFacade customerContextFacade, 
            IKlarnaService klarnaService, 
            ICartService cartService)
        {
            _klarnaService = klarnaService;
            _customerContextFacade = customerContextFacade;
            _cartService = cartService;
        }

        [Route("address/{addressId}")]
        [AcceptVerbs("GET")]
        public IHttpActionResult GetAddress(string addressId)
        {
            var address = _customerContextFacade.CurrentContact.ContactAddresses.FirstOrDefault(x => x.Name == addressId)?.ToAddress();
            return Ok(address);
        }

        [Route("personal/{billingAddressId}")]
        [AcceptVerbs("GET")]
        public IHttpActionResult GetpersonalInformation(string billingAddressId)
        {
            var cart = _cartService.LoadCart(_cartService.DefaultCartName);
            var sessionRequest = GetPersonalInformationSession(cart, billingAddressId);
            
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

        public virtual PersonalInformationSession GetPersonalInformationSession(ICart cart, string billingAddressId)
        {
            var request = new PersonalInformationSession();
            
            // Get customer info
            if (_klarnaService.Configuration.IsCustomerPreAssessmentEnabled)
            {
                request.Customer = new Customer
                {
                    DateOfBirth = "1980-01-01",
                    Gender = "Male",
                    LastFourSsn = "1234"
                };
            }

            // Get shipping address info
            var shipment = cart.GetFirstShipment();
            if (shipment?.ShippingAddress != null)
            {
                request.ShippingAddress = shipment.ShippingAddress.ToAddress();
            }

            // Get billling address info
            var billingAddress =
                _customerContextFacade.CurrentContact.ContactAddresses.FirstOrDefault(x => x.Name == billingAddressId)?
                    .ToAddress();
            request.BillingAddress = billingAddress;

            return request;
        }
    }
}