using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.DataAnnotations;
using EPiServer.Globalization;
using EPiServer.Shell.ObjectEditing;
using Foundation.Features.Shared;
using Foundation.Infrastructure;

namespace Foundation.Features.Blocks.KlarnaBlock
{
    [ContentType(DisplayName = "Klarna Block",
        GUID = "B5735027-04DE-44F0-A397-283C4DF46B9E",
        Description = "Klarna on-site messaging block",
        GroupName = GroupNames.Content)]
    [ImageUrl("/icons/cms/blocks/CMS-icon-block-03.png")]
    public class KlarnaBlock : FoundationBlockData
    {
        [SelectOne(SelectionFactoryType = typeof(KlarnaPlacementsSelectionFactory))]
        [Display(Name = "Placements", Order = 5)]
        public virtual string Placements { get; set; }

        public string GetLocale()
        {
            var culture = ContentLanguage.PreferredCulture;

            if (culture.Name.Equals("en", StringComparison.InvariantCultureIgnoreCase))
            {
                return "en-US";
            }

            return culture.Name;
        }
    }
}