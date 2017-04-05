using EPiServer.Commerce.Order;
using Klarna.Payments;
using Klarna.Payments.Models;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout
{
    public class DemoSessionBuilder : SessionBuilder
    {
        public override Session Build(Session session, ICart cart, Klarna.Payments.Configuration configuration)
        {
            if (configuration.IsCustomerPreAssessmentEnabled)
            {
                session.Customer = new Customer
                {
                    DateOfBirth = "1980-01-01",
                    Gender = "Male",
                    LastFourSsn = "1234"
                };
            }
            return session;
        }
    }
}