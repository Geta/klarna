using System.Configuration;
using System.Threading.Tasks;
using Klarna.Common;
using Klarna.Common.Models;
using Klarna.Payments.Models;
using Xunit;

namespace Test.Integration
{
    public class OrderManagementStoreTests
    {
        private readonly OrderManagementStore _orderManagementStore;

        public OrderManagementStoreTests()
        {
            _orderManagementStore = new OrderManagementStore(new ApiSession
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
        public async Task GetOrderReturnsOrder()
        {
            string orderId = "bba99d48-e988-16ce-9104-37673530b41f";
            var order = await _orderManagementStore.GetOrder(orderId);

            Assert.NotNull(order);
        }
        
        [Fact]
        public async Task GetRejectedOrderReturnsRejectedFraudStatus()
        {
            string orderId = "bba99d48-e988-16ce-9104-37673530b41f";
            var order = await _orderManagementStore.GetOrder(orderId);

            Assert.True(order.FraudStatus == OrderManagementFraudStatus.REJECTED);
        }
    }
}