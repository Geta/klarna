
using System;
using Mediachase.Commerce;

namespace Klarna.Common.Helpers
{
    public static class AmountHelper
    {
        public static int GetAmount(decimal amount)
        {
            return (int)Math.Round(amount * 100);
        }

        public static int GetAmount(Money money)
        {
            if (money.Amount > 0)
            {
                return GetAmount(money.Amount);
            }
            return 0;
        }
    }
}
