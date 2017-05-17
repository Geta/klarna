using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Klarna.Common.Helpers;
using Klarna.Common.Models;
using Mediachase.Commerce.Orders;

namespace Klarna.Common.Extensions
{
    public static class ShipmentExtensions
    {
#pragma warning disable 649
        private static Injected<IShippingCalculator> _shippingCalculator;
        private static Injected<ITaxCalculator> _taxCalculator;
#pragma warning restore 649

        public static PatchedOrderLine GetOrderLine(this IShipment shipment, ICart cart, OrderGroupTotals totals)
        {
            var shippingTaxTotal = _taxCalculator.Service.GetShippingTaxTotal(shipment, cart.Market, cart.Currency);

            var shippingTaxValue =  OrderContext.Current.GetTaxes(Guid.Empty, "General Sales", cart.Market.DefaultLanguage.Name, shipment.ShippingAddress).ToArray().FirstOrDefault(t => t.TaxType == TaxType.ShippingTax);

            var total = AmountHelper.GetAmount(totals.ShippingTotal);
            var totalTaxAmount = 0;
            var taxRate = 0;

            if (shippingTaxValue != null && shippingTaxTotal.Amount > 0)
            {
                totalTaxAmount = AmountHelper.GetAmount(shippingTaxTotal.Amount);
                taxRate = AmountHelper.GetAmount((decimal)shippingTaxValue.Percentage);

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