using System.Collections.Generic;
using EPiServer.Shell.ObjectEditing;

namespace Foundation.Features.Blocks.KlarnaBlock
{
    public class KlarnaPlacementsSelectionFactory : ISelectionFactory
    {
        public virtual IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return new ISelectItem[]
            {
                new SelectItem { Text = "footer-promotion-auto-size", Value = "footer-promotion-auto-size" },
                new SelectItem { Text = "credit-promotion-auto-size", Value = "credit-promotion-auto-size" },
                new SelectItem { Text = "sidebar-promotion-auto-size", Value = "sidebar-promotion-auto-size" },
                new SelectItem { Text = "top-strip-promotion-auto-size", Value = "top-strip-promotion-auto-size" },
                new SelectItem { Text = "credit-promotion-badge", Value = "credit-promotion-badge" },
                new SelectItem { Text = "info-page", Value = "info-page" },
                new SelectItem { Text = "top-strip-promotion-badge", Value = "top-strip-promotion-badge" },
                new SelectItem { Text = "homepage-promotion-tall", Value = "homepage-promotion-tall" },
                new SelectItem { Text = "homepage-promotion-wide", Value = "homepage-promotion-wide" },
                new SelectItem { Text = "homepage-promotion-box", Value = "homepage-promotion-box" },
            };
        }
    }
}