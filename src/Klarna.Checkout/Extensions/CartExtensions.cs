using System.Collections.Generic;
using EPiServer.Commerce.Order;

namespace Klarna.Checkout.Extensions
{
    public static class CartExtensions
    {
        public static void AddValidationIssues(
            this Dictionary<ILineItem, List<ValidationIssue>> issues,
            ILineItem lineItem,
            ValidationIssue issue)
        {
            if (!issues.ContainsKey(lineItem))
            {
                issues.Add(lineItem, new List<ValidationIssue>());
            }

            if (!issues[lineItem].Contains(issue))
            {
                issues[lineItem].Add(issue);
            }
        }
    }
}