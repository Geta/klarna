using System.Threading.Tasks;
using Klarna.OrderManagement.Models;
using Refit;

namespace Klarna.OrderManagement
{
    public interface IKlarnaOrderServiceApi
    {
        [Get("/ordermanagement/v1/orders/{order_id}")]
        Task<PatchedOrderData> GetOrder(string order_id);
    }
}
