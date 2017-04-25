using System;
using Klarna.Rest;
using Klarna.Rest.OrderManagement;
using Klarna.Rest.Transport;

namespace Klarna.OrderManagement
{
    public class KlarnaOrderService : IKlarnaOrderService
    {
        private readonly Client _client;

        public KlarnaOrderService(string merchantId, string sharedSecret, string apiUrl)
        {
            IConnector connector = ConnectorFactory.Create(merchantId, sharedSecret, new Uri(apiUrl));

            _client = new Client(connector);
        }

        public void CancelOrder(string orderId)
        {
            IOrder order = _client.NewOrder(orderId);
            order.Cancel();
        }
    }
}
