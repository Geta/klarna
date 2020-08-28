using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Klarna.Checkout;
using Klarna.Checkout.Models;
using Klarna.Common.Models;
using Newtonsoft.Json;
using Xunit;
using JsonSerializer = Klarna.Common.JsonSerializer;

namespace Test.Integration
{
    public class CheckoutStoreTests
    {
        private readonly CheckoutStore _checkoutStore;

        public CheckoutStoreTests()
        {
            _checkoutStore = new CheckoutStore(new ApiSession
            {
                ApiUrl = ConfigurationManager.AppSettings["Klarna:ApiUrl"],
                UserAgent = "Klarna Episerver Integration Tests",
                Credentials = new ApiCredentials
                {
                    Username = ConfigurationManager.AppSettings["Klarna:Username"],
                    Password = ConfigurationManager.AppSettings["Klarna:Password"]
                }
            }, new JsonSerializer());
        }
        
        [Fact]
        public async Task CreateOrderReturnsValidOrder()
        {
            var json = File.ReadAllText(Path.Combine(System.Environment.CurrentDirectory, "..\\..\\Data", "CheckoutCreateOrder.json"));
            var order = JsonConvert.DeserializeObject<CheckoutOrder>(json);

            var validOrder = await _checkoutStore.CreateOrder(order);
            Assert.NotNull(validOrder);
        }
        
        [Fact]
        public async Task GetOrderReturnsOrder()
        {
            string orderId = "302609b2-03b8-493a-906f-51ed21b51449";
            var order = await _checkoutStore.GetOrder(orderId);

            Assert.NotNull(order);
        }
    }
}