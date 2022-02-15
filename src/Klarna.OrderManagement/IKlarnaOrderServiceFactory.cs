using Klarna.Common.Configuration;

namespace Klarna.OrderManagement
{
    public interface IKlarnaOrderServiceFactory
    {
        IKlarnaOrderService Create(ConnectionConfiguration connectionConfiguration);
    }
}