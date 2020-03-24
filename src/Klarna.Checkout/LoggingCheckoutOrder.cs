using System.Threading.Tasks;
using EPiServer.Logging;
using Klarna.Rest.Core.Communication;
using Klarna.Rest.Core.Model;

namespace Klarna.Checkout
{
    public interface ICheckoutOrder
    {
        Task<CheckoutOrder> Create(CheckoutOrder order);
        Task<CheckoutOrder> Update(CheckoutOrder order);
        Task<CheckoutOrder> Fetch(string orderId);
    }

    public class LoggingCheckoutOrder : ICheckoutOrder
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(LoggingCheckoutOrder));
        private readonly Rest.Core.Klarna _client;

        public LoggingCheckoutOrder(Rest.Core.Klarna client)
        {
            _client = client;
        }

        public async Task<CheckoutOrder> Create(CheckoutOrder checkoutOrderData)
        {
            try
            {
                return await _client.Checkout.CreateOrder(checkoutOrderData).ConfigureAwait(false);
            }
            catch (ApiException e)
            {
                Log(e);
                throw;
            }
        }

        public async Task<CheckoutOrder> Update(CheckoutOrder checkoutOrderData)
        {
            try
            {
                return await _client.Checkout.UpdateOrder(checkoutOrderData).ConfigureAwait(false);
            }
            catch (ApiException e)
            {
                Log(e);
                throw;
            }
        }

        public async Task<CheckoutOrder> Fetch(string orderId)
        {
            try
            {

                return await _client.Checkout.GetOrder(orderId).ConfigureAwait(false);
            }
            catch (ApiException e)
            {
                Log(e);
                throw;
            }
        }

        private static void Log(ApiException e)
        {
            var messages = string.Join(" ", e.ErrorMessage.ErrorMessages);
            Logger.Error(
                $"Error Code: '{e.ErrorMessage.ErrorCode}'; CorelationId: '{e.ErrorMessage.CorrelationId}'; Messages: '{messages}'",
                e);
        }
    }
}