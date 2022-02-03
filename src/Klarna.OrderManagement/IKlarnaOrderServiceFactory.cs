using EPiServer.Commerce.Order;
using Klarna.Common;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;

namespace Klarna.OrderManagement
{
    public interface IKlarnaOrderServiceFactory
    {
        IKlarnaOrderService Create(IPayment payment, IMarket market);
        IKlarnaOrderService Create(PaymentMethodDto paymentMethodDto, MarketId marketMarketId);
        IKlarnaOrderService Create(ConnectionConfiguration connectionConfiguration);
    }
}