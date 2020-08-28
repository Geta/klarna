using System.Configuration;
using System.Threading.Tasks;
using Xunit;

namespace Test.Integration
{
    public class OrderManagementStoreTests
    {
        [Fact]
        public async Task GetOrderReturnsOrder()
        {
            var client = new Klarna.Common.Klarna(ConfigurationManager.AppSettings["Klarna:Username"], ConfigurationManager.AppSettings["Klarna:Password"], ConfigurationManager.AppSettings["Klarna:ApiUrl"]);
            var orderManagementStore = client.OrderManagement;

            string orderId = "bba99d48-e988-16ce-9104-37673530b41f";
            var order = await orderManagementStore.GetOrder(orderId);

            Assert.NotNull(order);
        }
    }
}