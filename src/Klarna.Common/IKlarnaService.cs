using EPiServer.Commerce.Order;
using Klarna.Common.Models;

namespace Klarna.Common
{
    public interface IKlarnaService
    {
        void FraudUpdate(NotificationModel notification);
    }
}