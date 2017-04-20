using System.Threading.Tasks;
using Klarna.Payments.Models;
using Refit;

namespace Klarna.Payments
{
    internal interface IKlarnaServiceApi
    {
        [Post("/credit/v1/sessions")]
        Task<CreateSessionResponse> CreatNewSession([Body]Session request);

        [Post("/credit/v1/sessions/{session_id}")]
        Task UpdateSession(string session_id, [Body]Session request);

        [Get("/credit/v1/sessions/{session_id}")]
        Task<Session> GetSession(string session_id);

        [Delete("/credit/v1/authorizations/{authorizationToken}")]
        Task CancelAuthorization(string authorizationToken);

        [Post("/credit/v1/authorizations/{authorizationToken}/order")]
        Task<CreateOrderResponse> CreateOrder(string authorizationToken, [Body]Session request);
    }
}
