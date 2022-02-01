using System;
using Klarna.Common;

namespace Klarna.Payments
{
    public class PaymentsConfiguration : ConnectionConfiguration
    {
        public bool CustomerPreAssessment { get; set; }
        public bool SendProductAndImageUrl { get; set; }
        public bool UseAttachments { get; set; }

        public bool AutoCapture { get; set; }

        [Obsolete]
        public string LogoUrl { get; set; }

        [Obsolete]
        public string WidgetButtonColor { get; set; }

        [Obsolete]
        public string WidgetButtonTextColor { get; set; }

        [Obsolete]
        public string WidgetCheckboxColor { get; set; }

        [Obsolete]
        public string WidgetCheckboxCheckmarkColor { get; set; }

        [Obsolete]
        public string WidgetHeaderColor { get; set; }

        [Obsolete]
        public string WidgetLinkColor { get; set; }

        public string WidgetBorderRadius { get; set; }
        public string WidgetDetailsColor { get; set; }
        public string WidgetBorderColor { get; set; }
        public string WidgetSelectedBorderColor { get; set; }
        public string WidgetTextColor { get; set; }

        [Obsolete]
        public string WidgetTextSecondaryColor { get; set; }

        public string ConfirmationUrl { get; set; }
        public string NotificationUrl { get; set; }
        public string PushUrl { get; set; }
        public string Design { get; set; }
    }
}
