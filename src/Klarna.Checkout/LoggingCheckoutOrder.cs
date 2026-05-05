using System.Threading.Tasks;
using EPiServer.Logging;
using Klarna.Checkout.Models;
using Klarna.Common.Models;

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
        private readonly CheckoutStore _client;

        public LoggingCheckoutOrder(CheckoutStore client)
        {
            _client = client;
        }

        public async Task<CheckoutOrder> Create(CheckoutOrder checkoutOrderData)
        {
            try
            {
                return await _client.CreateOrder(checkoutOrderData).ConfigureAwait(false);
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
                return await _client.UpdateOrder(checkoutOrderData).ConfigureAwait(false);
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

                return await _client.GetOrder(orderId).ConfigureAwait(false);
            }
            catch (ApiException e)
            {
                Log(e);
                throw;
            }
        }

        private static void Log(ApiException e)
        {
            try
            {
                var errorMessage = e?.ErrorMessage;
                if (errorMessage == null)
                {
                    Logger.Error(e?.Message, e);
                    return;
                }
                var messages = errorMessage.ErrorMessages != null
                    ? string.Join(" ", errorMessage.ErrorMessages)
                    : string.Empty;
                Logger.Error(
                    $"Error Code: '{errorMessage.ErrorCode}'; CorrelationId: '{errorMessage.CorrelationId}'; Messages: '{messages}'",
                    e);
            }
            catch
            {
                Logger.Error(e?.ToString(), e);
            }
        }
    }
}