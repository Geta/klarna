using EPiServer.Core;
using System.Collections.Generic;
using EPiServer.Recommendations.Commerce.Tracking;

namespace EPiServer.Reference.Commerce.Site.Features.Product.ViewModels
{
    public abstract class ProductViewModelBase
    {
        public IList<string> Images { get; set; }
        public IEnumerable<Recommendation> AlternativeProducts { get; set; }
        public IEnumerable<Recommendation> CrossSellProducts { get; set; }
    }
}