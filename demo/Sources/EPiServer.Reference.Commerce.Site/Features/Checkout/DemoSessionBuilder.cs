using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Order;
using Klarna.Payments.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Klarna.Payments;
using Klarna.Rest.Models;
using Customer = Klarna.Payments.Models.Customer;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout
{
    public class DemoSessionBuilder : ISessionBuilder
    {
        public Session Build(Session session, ICart cart, PaymentsConfiguration paymentsConfiguration, IDictionary<string, object> dic = null, bool includePersonalInformation = false)
        {
            if (includePersonalInformation && paymentsConfiguration.CustomerPreAssessment)
            {
                session.Customer = new Customer
                {
                    DateOfBirth = "1980-01-01",
                    Gender = "Male",
                    LastFourSsn = "1234"
                };
            }
            session.MerchantReference2 = "12345";

            if (paymentsConfiguration.UseAttachments)
            {
                var converter = new IsoDateTimeConverter
                {
                    DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"
                };


                var customerAccountInfos = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "unique_account_identifier",  "Test Testperson" },
                    { "account_registration_date", DateTime.Now },
                    { "account_last_modified", DateTime.Now }
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
            return session;
        }
    }
}