﻿using System;
using System.Collections.Generic;
using EPiServer.Commerce.Order;
using Klarna.Payments;
using Klarna.Payments.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout
{
    public class DemoSessionBuilder : SessionBuilder
    {
        public override Session Build(Session session, ICart cart, Klarna.Payments.Configuration configuration, bool includePersonalInformation = false)
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
            session.MerchantReference2 = "12345";

            if (configuration.UseAttachments)
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