using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Klarna.Payments.Models;

namespace Klarna.Payments
{
    public interface IKlarnaService
    {
        Task<string> CreateOrUpdateSession(ICart cart);
        string GetClientToken(ICart cart);
        string GetSessionId(ICart cart);
        Task<Session> GetSession(string sessionId);
        Session GetSessionRequest(ICart cart);
        Task<Authorization> GetAuthorizationModel(string sessionId);
        bool IsCustomerPreAssessmentEnabled();
        WidgetColorOptions GetWidgetColorOptions();

        void FraudUpdate(NotificationModel notification);

        void RedirectToConfirmationUrl(IPurchaseOrder purchaseOrder);

        Task<bool> UpdateBillingAddress(ICart cart, Address address);

        Task<bool> UpdateShippingAddress(ICart cart, Address address);

        Task CancelAuthorization(string authorizationToken);
        Task<CreateOrderResponse> CreateOrder(string authorizationToken, IOrderGroup cart);
    }
}