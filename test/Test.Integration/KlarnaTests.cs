using Klarna.Common.Models;
using Xunit;

namespace Test.Integration
{
    public class KlarnaTests
    {
        [Fact]
        public void Client_TestEULiveUrl()
        {
            var client = new Klarna.Common.Klarna(string.Empty, string.Empty, KlarnaEnvironment.LiveEurope);
            Assert.Equal("https://api.klarna.com/", client.ApiSession.ApiUrl);
        }

        [Fact]
        public void Client_TestNALiveUrl()
        {
            var client = new Klarna.Common.Klarna(string.Empty, string.Empty, KlarnaEnvironment.LiveNorthAmerica);
            Assert.Equal("https://api-na.klarna.com/", client.ApiSession.ApiUrl);
        }
        
        [Fact]
        public void Client_TestOceaniaLiveUrl()
        {
            var client = new Klarna.Common.Klarna(string.Empty, string.Empty, KlarnaEnvironment.LiveOceania);
            Assert.Equal("https://api-oc.klarna.com/", client.ApiSession.ApiUrl);
        }

        [Fact]
        public void Client_TestEUTestingUrl()
        {
            var client = new Klarna.Common.Klarna(string.Empty, string.Empty, KlarnaEnvironment.TestingEurope);
            Assert.Equal("https://api.playground.klarna.com/", client.ApiSession.ApiUrl);
        }

        [Fact]
        public void Client_TestNATestingUrl()
        {
            var client = new Klarna.Common.Klarna(string.Empty, string.Empty, KlarnaEnvironment.TestingNorthAmerica);
            Assert.Equal("https://api-na.playground.klarna.com/", client.ApiSession.ApiUrl);
        }
        
        [Fact]
        public void Client_TestOceaniaTestingUrl()
        {
            var client = new Klarna.Common.Klarna(string.Empty, string.Empty, KlarnaEnvironment.TestingOceania);
            Assert.Equal("https://api-oc.playground.klarna.com/", client.ApiSession.ApiUrl);
        }

        [Fact]
        public void Client_OrderManagementStoreInitiated()
        {
            var client = new Klarna.Common.Klarna(string.Empty, string.Empty, KlarnaEnvironment.TestingNorthAmerica);
            Assert.NotNull(client.OrderManagement);
        }
    }
}