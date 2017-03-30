using Klarna.Payments.Extensions;
using Mediachase.Commerce.Orders.Dto;

namespace Klarna.Payments.Models
{
    public class WidgetColorOptions
    {
        public string ColorDetails { get; set; }
        public string ColorButton { get; set; }
        public string ColorButtonText { get; set; }
        public string ColorCheckbox { get; set; }
        public string ColorCheckboxCheckmark { get; set; }
        public string ColorHeader { get; set; }
        public string ColorLink { get; set; }
        public string ColorBorder { get; set; }
        public string ColorBorderSelected { get; set; }
        public string ColorText { get; set; }
        public string ColorTextSecondary { get; set; }
        public string RadiusBorder { get; set; }

        public static WidgetColorOptions FromPaymentMethod(PaymentMethodDto paymentMethod)
        {
            return new WidgetColorOptions
            {
                ColorDetails = paymentMethod.GetParameter(Constants.KlarnaWidgetColorDetailsField, "#C0FFEE"),
                ColorButton = paymentMethod.GetParameter(Constants.KlarnaWidgetColorButtonField, "#C0FFEE"),
                ColorButtonText = paymentMethod.GetParameter(Constants.KlarnaWidgetColorButtonTextField, "#C0FFEE"),
                ColorCheckbox = paymentMethod.GetParameter(Constants.KlarnaWidgetColorCheckboxField, "#C0FFEE"),
                ColorCheckboxCheckmark = paymentMethod.GetParameter(Constants.KlarnaWidgetColorCheckboxCheckmarkField, "#C0FFEE"),
                ColorHeader = paymentMethod.GetParameter(Constants.KlarnaWidgetColorHeaderField, "#C0FFEE"),
                ColorLink = paymentMethod.GetParameter(Constants.KlarnaWidgetColorLinkField, "#C0FFEE"),
                ColorBorder = paymentMethod.GetParameter(Constants.KlarnaWidgetColorBorderField, "#C0FFEE"),
                ColorBorderSelected = paymentMethod.GetParameter(Constants.KlarnaWidgetColorBorderSelectedField, "#C0FFEE"),
                ColorText = paymentMethod.GetParameter(Constants.KlarnaWidgetColorTextField, "#C0FFEE"),
                ColorTextSecondary = paymentMethod.GetParameter(Constants.KlarnaWidgetColorTextSecondaryField, "#C0FFEE"),
                RadiusBorder = paymentMethod.GetParameter(Constants.KlarnaWidgetRadiusBorderField, "#0px")
            };
        }
    }
}
