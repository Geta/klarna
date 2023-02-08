using EPiServer.Commerce.Order;
using Mediachase.Commerce;

namespace Klarna.Common.Extensions
{
    public interface ILineItemTaxCalculator
    {
        decimal PriceIncludingTaxPercent(decimal basePrice, decimal taxPercent, IMarket market);
        decimal PriceIncludingTaxAmount(decimal basePrice, decimal taxAmount, IMarket market);
        ITaxValue[] GetTaxValuesForLineItem(ILineItem lineItem, IMarket market, IShipment shipment);
    }
}
