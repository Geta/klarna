using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using System.Collections.Generic;
using EPiServer.Recommendations.Commerce.Tracking;

namespace EPiServer.Reference.Commerce.Site.Features.Start.ViewModels
{
    public class StartPageViewModel
    {
        public StartPage StartPage { get; set; }
        public IEnumerable<PromotionViewModel> Promotions { get; set; }
        public IEnumerable<Recommendation> Recommendations { get; set; }
    }
}