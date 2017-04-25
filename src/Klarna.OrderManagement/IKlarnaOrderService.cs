namespace Klarna.OrderManagement
{
    public interface IKlarnaOrderService
    {
        void CancelOrder(string orderId);
    }
}