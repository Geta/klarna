using System.Linq;
using EPiServer.Commerce.Order;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Mediachase.Commerce;

namespace Klarna.Common.Extensions
{
    public static class ShipmentExtensions
    {
        public static PatchedOrderLine GetOrderLine(this IShipment shipment, OrderGroupTotals totals, Money shippingTaxTotal)
        {
            /*
           unit_price integer required
               Minor units. Includes tax, excludes discount. (max value: 100000000)
           tax_rate integer required
               Non-negative. In percent, two implicit decimals. I.e 2500 = 25%.
           total_amount integer required
               Includes tax and discount. Must match (quantity * unit_price) - total_discount_amount within ±quantity. (max value: 100000000)
           total_tax_amount integer required
               Must be within ±1 of total_amount - total_amount * 10000 / (10000 + tax_rate). Negative when type is discount.
           total_discount_amount integer
               Non - negative minor units. Includes tax.
           */

            var priceIncludingTax = totals.ShippingTotal + shippingTaxTotal;

            var shipmentOrderLine = new PatchedOrderLine
            {
                Name = shipment.ShippingMethodName,
                Quantity = 1,
                UnitPrice = AmountHelper.GetAmount(priceIncludingTax),
                TotalAmount = AmountHelper.GetAmount(priceIncludingTax),
                // TODO Get ITaxValue percentage, check if it also works with payments
                TaxRate = AmountHelper.GetAmount(100 * (priceIncludingTax.Amount / totals.ShippingTotal.Amount - 1)),
                TotalTaxAmount = AmountHelper.GetAmount(shippingTaxTotal),
            Type = "shipping_fee"
            };
            if (string.IsNullOrEmpty(shipmentOrderLine.Name))
            {
                var shipmentMethod = Mediachase.Commerce.Orders.Managers.ShippingManager.GetShippingMethod(shipment.ShippingMethodId)
                    .ShippingMethod.FirstOrDefault();
                if (shipmentMethod != null)
                {
                    shipmentOrderLine.Name = shipmentMethod.DisplayName;
                }
            }
            return shipmentOrderLine;
        }
    }
}