using System.Threading.Tasks;
using Klarna.Common;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Klarna.Payments.Models;

namespace Klarna.Payments
{
    /// <summary>
    /// The payments API is used to create a session to offer Klarna's payment methods as part of your checkout.
    /// As soon as the purchase is completed the order should be read and handled using the Order Management API.
    /// </summary>
    public class PaymentsStore : BaseStore
    {
        public PaymentsStore(ApiSession apiSession, IJsonSerializer jsonSerializer) :
            base(apiSession, "payments/v1/sessions", jsonSerializer)
        { }

        /// <summary>
        /// Creates a new credit session
        /// <a href="https://developers.klarna.com/api/#payments-api-create-a-new-credit-session">
        ///     https://developers.klarna.com/api/#payments-api-create-a-new-credit-session
        /// </a>
        /// </summary>
        /// <param name="creditSession">The <see cref="PaymentCreditSession"/>object</param>
        /// <returns><see cref="PaymentCreditSessionResponse"/></returns>
        public async Task<CreateSessionResponse> CreateSession(Session creditSession)
        {
            var url = ApiUrlHelper.GetApiUrlForController(ApiSession.ApiUrl, ApiControllerUri);
            return await Post<CreateSessionResponse>(url, creditSession).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads an existing credit session
        /// <a href="https://developers.klarna.com/api/#payments-api-read-an-existing-credit-session">
        ///     https://developers.klarna.com/api/#payments-api-read-an-existing-credit-session
        /// </a>
        /// </summary>
        /// <param name="sessionId">Id of the credit session to retrieve</param>
        /// <returns><see cref="Session"/></returns>
        public async Task<Session> GetSession(string sessionId)
        {
            var url = ApiUrlHelper.GetApiUrlForController(ApiSession.ApiUrl, ApiControllerUri, sessionId);
            return await Get<Session>(url).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an existing credit session
        /// <a href="https://developers.klarna.com/api/#payments-api-update-an-existing-credit-session">
        ///     https://developers.klarna.com/api/#payments-api-update-an-existing-credit-session
        /// </a>
        /// </summary>
        /// <param name="sessionId">Id of the credit session to update</param>
        /// <param name="creditSession">The <see cref="Session"/> object</param>
        /// <returns></returns>
        public Task UpdateSession(string sessionId, Session creditSession)
        {
            var url = ApiUrlHelper.GetApiUrlForController(ApiSession.ApiUrl, ApiControllerUri, sessionId);
            return Post(url, creditSession);
        }

        /// <summary>
        /// Creates a new order
        /// <a href="https://developers.klarna.com/api/#payments-api-create-a-new-order">
        ///     https://developers.klarna.com/api/#payments-api-create-a-new-order
        /// </a>
        /// </summary>
        /// <param name="authorizationToken">Authorization token from JS client</param>
        /// <param name="session">The <see cref="Session"/> object</param>
        /// <returns><see cref="CreateOrderResponse"/></returns>
        public async Task<CreateOrderResponse> CreateOrder(string authorizationToken, Session session)
        {
            var url = ApiUrlHelper.GetApiUrlForController(ApiSession.ApiUrl,
                "payments/v1/authorizations",
                $"{authorizationToken}/order");
            return await Post<CreateOrderResponse>(url, session).ConfigureAwait(false);
        }
    }
}