using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Mediachase.Commerce.Orders.Dto;

namespace Klarna.Checkout
{
    public static class ShippingMethodExtensions
    {
        public static ShippingOption ToShippingOption(this ShippingMethodDto.ShippingMethodRow method)
        {
            return new ShippingOption
            {
                Id = method.ShippingMethodId.ToString(),
                Name = method.DisplayName,
                Price = AmountHelper.GetAmount(method.BasePrice),
                Preselected = method.IsDefault,
                TaxAmount = 0,
                TaxRate = 0,
                Description = method.Description
            };
        }
    }
}