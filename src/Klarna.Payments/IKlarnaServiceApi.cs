using System.Threading.Tasks;
using Klarna.Payments.Models;
using Refit;

namespace Klarna.Payments
{
    public interface IKlarnaServiceApi
    {
        [Post("/payments/v1/sessions")]
        Task<CreateSessionResponse> CreatNewSession([Body]Session request);

        [Post("/payments/v1/sessions/{session_id}")]
        Task UpdateSession(string session_id, [Body]Session request);

        [Get("/payments/v1/sessions/{session_id}")]
        Task<Session> GetSession(string session_id);

        [Delete("/payments/v1/authorizations/{authorizationToken}")]
        Task CancelAuthorization(string authorizationToken);

        [Post("/payments/v1/authorizations/{authorizationToken}/order")]
        Task<CreateOrderResponse> CreateOrder(string authorizationToken, [Body]Session request);
    }
}
