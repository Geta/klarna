using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Klarna.Payments.Models;

namespace Klarna.Payments
{
    public interface IKlarnaService
    {
        Task<string> CreateSession(Session request);
        Task<string> CreateSession(ICart cart);
        Task<string> CreateOrUpdateSession(ICart cart);
        string GetClientToken(ICart cart);
        Task<Session> GetSession(string sessionId);
        WidgetColorOptions GetWidgetColorOptions();

        Task CancelAuthorization(string authorizationToken);
        Task<CreateOrderResponse> CreateOrder(string authorizationToken, ICart cart);
    }
}