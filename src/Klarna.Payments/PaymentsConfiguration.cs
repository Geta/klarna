using System.Collections.Generic;
using Klarna.Common;

namespace Klarna.Payments
{
    public class PaymentsConfiguration : ConnectionConfiguration
    {
        public bool CustomerPreAssessment { get; set; }
        public bool IsProduction { get; set; }
        public bool SendProductAndImageUrlField { get; set; }
        public bool UseAttachments { get; set; }

        public string LogoUrl { get; set; }
        public string WidgetButtonColor { get; set; }
        public string WidgetButtonTextColor { get; set; }
        public string WidgetCheckboxColor { get; set; }
        public string WidgetCheckboxCheckmarkColor { get; set; }
        public string WidgetHeaderColor { get; set; }
        public string WidgetLinkColor { get; set; }
        public string WidgetBorderRadius { get; set; }
        public string WidgetDetailsColor { get; set; }
        public string WidgetBorderColor { get; set; }
        public string WidgetSelectedBorderColor { get; set; }
        public string WidgetTextColor { get; set; }
        public string WidgetTextSecondaryColor { get; set; }

        public string ConfirmationUrl { get; set; }
        public string NotificationUrl { get; set; }
        public string PushUrl { get; set; }
    }
}
