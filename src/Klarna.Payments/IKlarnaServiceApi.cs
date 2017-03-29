using System.Threading.Tasks;
using Klarna.Payments.Models;
using Refit;

namespace Klarna.Payments
{
    public interface IKlarnaServiceApi
    {
        [Post("/credit/v1/sessions")]
        Task<CreateSessionResponse> CreatNewSession([Body]CreateSessionRequest request);

        [Get("/credit/v1/sessions/{session_id}")]
        Task<CreateSessionRequest> GetSession(string session_id);

        [Post("/credit/v1/authorizations/{authorizationToken}/order")]
        Task CreateOrder(string authorizationToken, [Body]CreateSessionRequest request);
    }
}
