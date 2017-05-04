using EPiServer.Commerce.Order;
using Klarna.Payments.Models;

namespace Klarna.Payments
{
    public interface ISessionBuilder
    {
        Session Build(Session session, ICart cart, Configuration configuration, bool includePersonalInformation = false);
    }
}
