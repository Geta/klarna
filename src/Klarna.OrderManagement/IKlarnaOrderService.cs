namespace Klarna.OrderManagement
{
    public interface IKlarnaOrderService
    {
        void CancelOrder(string orderId);

        void UpdateMerchantReferences(string orderId, string merchantReference1, string merchantReference2);
    }
}