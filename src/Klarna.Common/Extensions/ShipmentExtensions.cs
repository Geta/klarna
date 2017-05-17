using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Klarna.Common.Helpers;
using Klarna.Common.Models;

namespace Klarna.Common.Extensions
{
    public static class ShipmentExtensions
    {
#pragma warning disable 649
        private static Injected<ITaxCalculator> _taxCalculator;
#pragma warning restore 649

        public static PatchedOrderLine GetOrderLine(this IShipment shipment, ICart cart, OrderGroupTotals totals)
        {
            var shippingTaxTotal = _taxCalculator.Service.GetShippingTaxTotal(shipment, cart.Market, cart.Currency);
            var total = AmountHelper.GetAmount(totals.ShippingTotal);
            var totalTaxAmount = 0;
            var taxRate = 0;

            if (shippingTaxTotal.Amount > 0)
            {
                totalTaxAmount = AmountHelper.GetAmount(shippingTaxTotal.Amount);
                taxRate = AmountHelper.GetAmount((shippingTaxTotal.Amount / totals.ShippingTotal.Amount) * 100);

                total = total + totalTaxAmount;
            }

            var shipmentOrderLine = new PatchedOrderLine
            {
                Name = shipment.ShippingMethodName,
                Quantity = 1,
                UnitPrice = total,
                TotalAmount = total,
                TaxRate = taxRate,
                TotalTaxAmount = totalTaxAmount,
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

        public static PatchedOrderLine GetOrderLine(this IShipment shipment, OrderGroupTotals totals)
        {
            var shipmentOrderLine = new PatchedOrderLine
            {
                Name = shipment.ShippingMethodName,
                Quantity = 1,
                UnitPrice = AmountHelper.GetAmount(totals.ShippingTotal),
                TotalAmount = AmountHelper.GetAmount(totals.ShippingTotal),
                TaxRate = 0,
                TotalTaxAmount = 0,
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