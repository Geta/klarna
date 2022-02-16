# Klarna On-site messsaging

On-site messaging is a platform that enables you to add tailored messaging to your website. With On-site messaging you can let shoppers know about the different payment options you have available as they browse your site. By using Klarna, customers have access to our flexible payment options in the checkout; On-site messaging is a great way to let them know even before they decide to buy.

There are assets designed for all of the relevant pages of your website. All of these assets are available in the Merchant Portal and are free to use. Every asset is dynamic, adjusting to the payment methods that you offer.

[More information](https://docs.klarna.com/on-site-messaging/overview/).

## Installation

Before installing On-site messaging you need to [activate it in the Merchant Portal](https://docs.klarna.com/on-site-messaging/get-started/activation/). 

Once activated you can add the JavaScript Library needed to run it. The JavaScript Library snippet is generated for you and can be obtained in the installation page in the On-site messaging panel in the Merchant Portal. Log in to the Merchant Portal and go to the On-site messaging application. Once there, you will be redirected to the Installation section of the application.

## Sample code

We've included a sample block below that uses the default placement options. Note that this requires the JavaScript library to work (obtained under installation).

KlarnaBlock.cs
```
using System;
using System.Collections.Generic;
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
```

KlarnaBlock.cshtml
```
@using Foundation.Features.Blocks.KlarnaBlock

@model IBlockViewModel<KlarnaBlock>

@Html.FullRefreshPropertiesMetaData(new[] { "Placements" })

<div style="background-color: @Model.CurrentBlock.BackgroundColor; opacity: @Model.CurrentBlock.BlockOpacity;" class="klarna-block @(Model.CurrentBlock.Padding + " " + Model.CurrentBlock.Margin)">
    <klarna-placement data-key="@Model.CurrentBlock.Placements"
                      data-locale="@Model.CurrentBlock.GetLocale()"></klarna-placement>
</div>
```