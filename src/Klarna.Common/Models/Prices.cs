namespace Klarna.Common.Models
{
    public class Prices
    {
        public int UnitPrice { get; }
        public int TaxRate { get; }
        public int TotalDiscountAmount { get; }
        public int TotalAmount { get; }
        public int TotalTaxAmount { get; }

        public Prices(int unitPrice, int taxRate, int totalDiscountAmount, int totalAmount, int totalTaxAmount)
        {
            UnitPrice = unitPrice;
            TaxRate = taxRate;
            TotalDiscountAmount = totalDiscountAmount;
            TotalAmount = totalAmount;
            TotalTaxAmount = totalTaxAmount;
        }
    }
}