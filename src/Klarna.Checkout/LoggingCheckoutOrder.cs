using System;
using EPiServer.Logging;
using Klarna.Rest.Core.Communication;
using Klarna.Rest.Core.Model;

namespace Klarna.Checkout
{
    public interface ICheckoutOrder
    {
        void Create(CheckoutOrder order);
        CheckoutOrder Update(CheckoutOrder order);
        CheckoutOrder Fetch(string orderId);
    }

    public class LoggingCheckoutOrder : ICheckoutOrder
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(LoggingCheckoutOrder));
        private readonly Rest.Core.Klarna _client;

        public LoggingCheckoutOrder(Rest.Core.Klarna client)
        {
            _client = client;
        }

        private readonly ICheckoutOrder _inner;

        public LoggingCheckoutOrder(ICheckoutOrder inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public void Create(CheckoutOrder checkoutOrderData)
        {
            try
            {
                _client.Checkout.CreateOrder(checkoutOrderData);
            }
            catch (ApiException e)
            {
                Log(e);
                throw;
            }
        }

        public CheckoutOrder Update(CheckoutOrder checkoutOrderData)
        {
            try
            {
                return _client.Checkout.UpdateOrder(checkoutOrderData).Result;
            }
            catch (ApiException e)
            {
                Log(e);
                throw;
            }
        }

        public CheckoutOrder Fetch(string orderId)
        {
            try
            {

                return _client.Checkout.GetOrder(orderId).Result;
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