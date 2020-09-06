using System;
using System.Net.Http;
using System.Text;
using Klarna.Common.Models;
using Klarna.Payments;
using Klarna.Payments.Models;
using Refit;
using Xunit;

namespace Test.Integration
{
    public class KlarnaPaymentTests
    {
        private string _apiUrl = "";
        private string _username = "";
        private string _password = "";

        private string _tempSessionId = "";

        private IKlarnaServiceApi _klarnaServiceApi;

        public IKlarnaServiceApi KlarnaServiceApi
        {
            get
            {
                if (_klarnaServiceApi == null)
                {
                    var byteArray = Encoding.ASCII.GetBytes($"{_username}:{_password}");
                    var httpClient = new HttpClient
                    {
                        BaseAddress = new Uri(_apiUrl)
                    };
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    var refitSettings = new RefitSettings();

                    _klarnaServiceApi = RestService.For<IKlarnaServiceApi>(httpClient, refitSettings);
                }
                return _klarnaServiceApi;
            }
        }

        [Fact]
        public void CreateSession()
        {
            var session = GetSession();

            try
            {
                var result = KlarnaServiceApi.CreatNewSession(session).Result;

                Assert.False(true);
            }
            catch (Exception)
            {
                Assert.False(true);
            }
        }

        [Fact]
        public void UpdateSession()
        {
            var session = GetSession();

            try
            {
                KlarnaServiceApi.UpdateSession(_tempSessionId, session);

                Assert.False(true);
            }
            catch (Exception)
            {
                Assert.False(true);
            }
        }

        private Session GetSession()
        {
            var session = new Session();
            session.Design = null;
            session.PurchaseCountry = "US";
            session.PurchaseCurrency = "USD";
            session.Locale = "en-ud";
            session.OrderAmount = 1000;
            session.OrderTaxAmount = 0;
            session.OrderLines = new []
            {
                new OrderLine { Name = "Product a", Quantity = 1, TotalAmount = 1000, UnitPrice = 1000 }
            };
            session.Customer = null;
            session.MerchantUrl = new MerchantUrl
            {
                Confirmation = "http://localhost:9000",
                Notification = "http://localhost:9000"
            };
            session.MerchantReference1 = null;
            session.MerchantReference2 = null;
            session.MerchantData = null;
            session.Body = null;
            session.Options = null;

            return session;
        }
    }
}
