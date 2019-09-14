using System.Collections.Generic;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Customers;

namespace Klarna.Checkout
{
    [ServiceConfiguration(typeof(IKlarnaCartValidator))]
    public class DefaultKlarnaCartValidator : IKlarnaCartValidator
    {
        private readonly ILineItemValidator _lineItemValidator;
        private readonly IPlacedPriceProcessor _placedPriceProcessor;
        private readonly IInventoryProcessor _inventoryProcessor;
        private readonly CustomerContext _customerContext;
        private readonly IPromotionEngine _promotionEngine;

        public DefaultKlarnaCartValidator(
            ILineItemValidator lineItemValidator,
            IPlacedPriceProcessor placedPriceProcessor,
            IInventoryProcessor inventoryProcessor,
            CustomerContext customerContext,
            IPromotionEngine promotionEngine)
        {
            _lineItemValidator = lineItemValidator;
            _placedPriceProcessor = placedPriceProcessor;
            _inventoryProcessor = inventoryProcessor;
            _customerContext = customerContext;
            _promotionEngine = promotionEngine;
        }
        
        public virtual Dictionary<ILineItem, List<ValidationIssue>> ValidateCart(ICart cart)
        {
            var validationIssues = new Dictionary<ILineItem, List<ValidationIssue>>();
            cart.ValidateOrRemoveLineItems((item, issue) => validationIssues.AddValidationIssues(item, issue), _lineItemValidator);
            cart.UpdatePlacedPriceOrRemoveLineItems(_customerContext.GetContactById(cart.CustomerId), (item, issue) => validationIssues.AddValidationIssues(item, issue), _placedPriceProcessor);
            cart.UpdateInventoryOrRemoveLineItems((item, issue) => validationIssues.AddValidationIssues(item, issue), _inventoryProcessor);

            return validationIssues;
        }
        
        public virtual IEnumerable<RewardDescription> ApplyDiscounts(ICart cart, RequestFulfillmentStatus requestedStatuses = RequestFulfillmentStatus.Fulfilled)
        {
            return cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings
            {
                RequestedStatuses = requestedStatuses,
                ExclusionLevel = ExclusionLevel.Unit
            });
        }
    }
}