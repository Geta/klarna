using Klarna.Payments.Extensions;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;

namespace Klarna.Payments.CommerceManager.Apps.Order.Payments.Plugins.KlarnaPayments
{
    public partial class ConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        public string ValidationGroup { get; set; }

        public void LoadObject(object dto)
        {
            var paymentMethod = dto as PaymentMethodDto;

            if (paymentMethod == null)
            {
                return;
            }
            txtUsername.Text = paymentMethod.GetParameter(Constants.KlarnaUsernameField, string.Empty);
            txtPassword.Text = paymentMethod.GetParameter(Constants.KlarnaPasswordField, string.Empty);
            var isProduction = bool.Parse(paymentMethod.GetParameter(Constants.KlarnaIsProductionField, "false"));
            IsProductionCheckBox.Checked = isProduction;

            txtColorDetails.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorDetailsField, string.Empty);
            txtColorButton.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorButtonField, string.Empty);
            txtColorButtonText.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorButtonTextField, string.Empty);
            txtColorCheckbox.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorCheckboxField, string.Empty);
            txtColorCheckboxCheckmark.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorCheckboxCheckmarkField, string.Empty);
            txtColorHeader.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorHeaderField, string.Empty);
            txtColorLink.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorLinkField, string.Empty);
            txtColorBorder.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorBorderField, string.Empty);
            txtColorBorderSelected.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorBorderSelectedField, string.Empty);
            txtColorText.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorTextField, string.Empty);
            txtColorTextSecondary.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorTextSecondaryField, string.Empty);
            txtRadiusBorder.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetRadiusBorderField, string.Empty);

            txtConfirmationUrl.Text = paymentMethod.GetParameter(Constants.ConfirmationUrlField, string.Empty);
            txtNotificationUrl.Text = paymentMethod.GetParameter(Constants.NotificationUrlField, string.Empty);
            var sendProductAndImageUrl = bool.Parse(paymentMethod.GetParameter(Constants.SendProductAndImageUrlField, "false"));
            SendProductAndImageUrlCheckBox.Checked = sendProductAndImageUrl;
            var useAttachments = bool.Parse(paymentMethod.GetParameter(Constants.UseAttachmentsField, "false"));
            UseAttachmentsCheckBox.Checked = useAttachments;
            var preAssesment = bool.Parse(paymentMethod.GetParameter(Constants.PreAssesmentField, "false"));
            PreAssesmentCheckBox.Checked = preAssesment;
            txtNameOfCreditForm.Text = paymentMethod.GetParameter(Constants.NameOfCreditFormField, string.Empty);
        }

        public void SaveChanges(object dto)
        {
            if (!Visible)
            {
                return;
            }

            var paymentMethod = dto as PaymentMethodDto;
            if (paymentMethod == null)
            {
                return;
            }
            paymentMethod.SetParameter(Constants.KlarnaUsernameField, txtUsername.Text);
            paymentMethod.SetParameter(Constants.KlarnaPasswordField, txtPassword.Text);
            paymentMethod.SetParameter(Constants.KlarnaIsProductionField, (IsProductionCheckBox.Checked ? "true" : "false"));

            paymentMethod.SetParameter(Constants.KlarnaWidgetColorDetailsField, txtColorDetails.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorButtonField, txtColorButton.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorButtonTextField, txtColorButtonText.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorCheckboxField, txtColorCheckbox.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorCheckboxCheckmarkField, txtColorCheckboxCheckmark.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorHeaderField, txtColorHeader.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorLinkField, txtColorLink.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorBorderField, txtColorBorder.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorBorderSelectedField, txtColorBorderSelected.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorTextField, txtColorText.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorTextSecondaryField, txtColorTextSecondary.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetRadiusBorderField, txtRadiusBorder.Text);

            paymentMethod.SetParameter(Constants.ConfirmationUrlField, txtConfirmationUrl.Text);
            paymentMethod.SetParameter(Constants.NotificationUrlField, txtNotificationUrl.Text);
            paymentMethod.SetParameter(Constants.SendProductAndImageUrlField, (SendProductAndImageUrlCheckBox.Checked ? "true" : "false"));
            paymentMethod.SetParameter(Constants.UseAttachmentsField, (UseAttachmentsCheckBox.Checked ? "true" : "false"));
            paymentMethod.SetParameter(Constants.PreAssesmentField, (PreAssesmentCheckBox.Checked ? "true" : "false"));
            paymentMethod.SetParameter(Constants.NameOfCreditFormField, txtNameOfCreditForm.Text);
        }
        
    }
}