using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Klarna.Payments.Models;

namespace Klarna.Payments
{
    public interface IKlarnaService
    {
        Task<string> CreateOrUpdateSession(Session sessionRequest, ICart cart);
        Session GetSessionRequest(ICart cart);
        string GetClientToken(ICart cart);
        Task<Session> GetSession(string sessionId);
        bool IsCustomerPreAssessmentEnabled();
        WidgetColorOptions GetWidgetColorOptions();

        Task CancelAuthorization(string authorizationToken);
        Task<CreateOrderResponse> CreateOrder(string authorizationToken, ICart cart);
    }
}