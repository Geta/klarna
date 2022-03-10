using System.Collections.Generic;
using EPiServer.Commerce.Order;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Klarna.Common.Configuration;
using Klarna.Common.Models;
using Klarna.Payments;
using Klarna.Payments.Models;
using Mediachase.Commerce.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Foundation.Features.Checkout
{
    [ServiceConfiguration(typeof(ISessionBuilder), Lifecycle = ServiceInstanceScope.Transient)]
    public class KlarnaDemoSessionBuilder : ISessionBuilder
    {
        public Session Build(Session session, ICart cart, PaymentsConfiguration paymentsConfiguration, IDictionary<string, object> dic = null, bool includePersonalInformation = false)
        {
            // This is for demo purpose only since we want to show B2B features for logged in users in the German market
            if (PrincipalInfo.CurrentPrincipal.Identity.IsAuthenticated && paymentsConfiguration.MarketId == "DEU")
            {
                session.Customer = new Customer
                {
                    DateOfBirth = "1980-01-01",
                    Gender = "Male",
                    LastFourSsn = "1234",
                    Type = "organization"
                };
            }
            session.MerchantReference2 = "12345";

            if (paymentsConfiguration.UseAttachments && PrincipalInfo.CurrentPrincipal.Identity.IsAuthenticated)
            {
                var converter = new IsoDateTimeConverter
                {
                    DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"
                };

                var customerContact = PrincipalInfo.CurrentPrincipal.GetCustomerContact();

                var customerAccountInfos = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            { "unique_account_identifier",  PrincipalInfo.CurrentPrincipal.GetContactId() },
                            { "account_registration_date", customerContact.Created },
                            { "account_last_modified", customerContact.Modified }
                        }
                    };

                var emd = new Dictionary<string, object>
                    {
                        { "customer_account_info", customerAccountInfos}
                    };

                session.Attachment = new Attachment
                {
                    ContentType = "application/vnd.klarna.internal.emd-v2+json",
                    Body = JsonConvert.SerializeObject(emd, converter)
                };
            }

            if (session.OrderLines != null)
            {
                foreach (var lineItem in session.OrderLines)
                {
                    if (lineItem.Type.Equals("physical"))
                    {
                        if (lineItem.ProductIdentifiers == null)
                        {
                            lineItem.ProductIdentifiers = new ProductIdentifiers();
                        }

                        lineItem.ProductIdentifiers.GlobalTradeItemNumber = "GlobalTradeItemNumber test";
                        lineItem.ProductIdentifiers.ManufacturerPartNumber = "ManuFacturerPartNumber test";
                        lineItem.ProductIdentifiers.CategoryPath = "test / test";
                    }
                }
            }
            return session;
        }
    }
}