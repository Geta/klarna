using System;
using EPiServer.Logging;
using Klarna.Rest.Checkout;
using Klarna.Rest.Models;
using Klarna.Rest.Transport;

namespace Klarna.Checkout
{
    public class LoggingCheckoutOrder : ICheckoutOrder
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(LoggingCheckoutOrder));

        private readonly ICheckoutOrder _inner;

        public LoggingCheckoutOrder(ICheckoutOrder inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public Uri Location
        {
            get => _inner.Location;
            set => _inner.Location = value;
        }

        public void Create(CheckoutOrderData checkoutOrderData)
        {
            try
            {
                _inner.Create(checkoutOrderData);
            }
            catch (ApiException e)
            {
                Log(e);
                throw;
            }
        }

        public CheckoutOrderData Update(CheckoutOrderData checkoutOrderData)
        {
            try
            {
                return _inner.Update(checkoutOrderData);
            }
            catch (ApiException e)
            {
                Log(e);
                throw;
            }
        }

        public CheckoutOrderData Fetch()
        {
            try
            {
                return _inner.Fetch();
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