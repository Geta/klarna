using System.Collections.Generic;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;

namespace Klarna.Checkout
{
    public interface IKlarnaCartValidator
    {
        Dictionary<ILineItem, List<ValidationIssue>> ValidateCart(ICart cart);
        IEnumerable<RewardDescription> ApplyDiscounts(ICart cart, RequestFulfillmentStatus requestedStatuses = RequestFulfillmentStatus.Fulfilled);
    }
}