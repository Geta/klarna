using System.Threading.Tasks;
using Klarna.Payments.Models;

namespace Klarna.Payments
{
    public class KlarnaService
    {
        private readonly IKlarnaServiceApi _klarnaServiceApi;

        public KlarnaService(IKlarnaServiceApi klarnaServiceApi)
        {
            _klarnaServiceApi = klarnaServiceApi;
        }

        public async Task<string> CreateSession()
        {
            var request = new CreateSessionRequest();


            var response = await _klarnaServiceApi.CreatNewSession(request);

            return response.ClientToken;
        }

        public async Task<CreateSessionRequest> GetSession(string sessionId)
        {
            return await _klarnaServiceApi.GetSession(sessionId);
        }

        public 
    }
}
