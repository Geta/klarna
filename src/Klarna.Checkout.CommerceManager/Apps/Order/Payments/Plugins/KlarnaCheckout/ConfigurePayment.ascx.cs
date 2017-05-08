using Klarna.Common.Extensions;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;

namespace Klarna.Checkout.CommerceManager.Apps.Order.Payments.Plugins.KlarnaCheckout
{
    public partial class ConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        private PaymentMethodDto _paymentMethodDto;

        public string ValidationGroup { get; set; }

        public void LoadObject(object dto)
        {
            var paymentMethod = dto as PaymentMethodDto;

            if (paymentMethod == null)
            {
                return;
            }
            _paymentMethodDto = paymentMethod;

            txtUsername.Text = paymentMethod.GetParameter(Common.Constants.KlarnaUsernameField, string.Empty);
            txtPassword.Text = paymentMethod.GetParameter(Common.Constants.KlarnaPasswordField, string.Empty);
            txtApiUrl.Text = paymentMethod.GetParameter(Common.Constants.KlarnaApiUrlField, string.Empty);
            
            txtColorButton.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorButtonField, string.Empty);
            txtColorButtonText.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorButtonTextField, string.Empty);
            txtColorCheckbox.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorCheckboxField, string.Empty);
            txtColorHeader.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorHeaderField, string.Empty);
            txtColorLink.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorLinkField, string.Empty);
            txtRadiusBorder.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetRadiusBorderField, string.Empty);
            txtColorCheckboxCheckmark.Text = paymentMethod.GetParameter(Constants.KlarnaWidgetColorCheckboxCheckmarkField, string.Empty);
            var allowSeparateShippingAddress = bool.Parse(paymentMethod.GetParameter(Constants.AllowSeparateShippingAddressField, "false"));
            allowSeparateShippingAddressCheckBox.Checked = allowSeparateShippingAddress;
            var dateOfBirthMandatory = bool.Parse(paymentMethod.GetParameter(Constants.DateOfBirthMandatoryField, "false"));
            dateOfBirthMandatoryCheckBox.Checked = dateOfBirthMandatory;
            txtShippingDetails.Text = paymentMethod.GetParameter(Constants.ShippingDetailsField, string.Empty);
            var titleMandatory = bool.Parse(paymentMethod.GetParameter(Constants.TitleMandatoryField, "false"));
            titleMandatoryCheckBox.Checked = titleMandatory;
            var showSubtotalDetail = bool.Parse(paymentMethod.GetParameter(Constants.ShowSubtotalDetailField, "false"));
            showSubtotalDetailCheckBox.Checked = showSubtotalDetail;
            var requireValidateCallbackSuccess = bool.Parse(paymentMethod.GetParameter(Constants.RequireValidateCallbackSuccessField, "false"));
            requireValidateCallbackSuccessCheckBox.Checked = requireValidateCallbackSuccess;
            additionalCheckboxTextTextBox.Text = paymentMethod.GetParameter(Constants.AdditionalCheckboxTextField, string.Empty);
            var additionalCheckboxDefaultChecked = bool.Parse(paymentMethod.GetParameter(Constants.AdditionalCheckboxDefaultCheckedField, "false"));
            additionalCheckboxDefaultCheckedCheckBox.Checked = additionalCheckboxDefaultChecked;
            var additionalCheckboxRequired = bool.Parse(paymentMethod.GetParameter(Constants.AdditionalCheckboxRequiredField, "false"));
            additionalCheckboxRequiredCheckBox.Checked = additionalCheckboxRequired;

            txtConfirmationUrl.Text = paymentMethod.GetParameter(Constants.ConfirmationUrlField, string.Empty);
            txtTermsUrl.Text = paymentMethod.GetParameter(Constants.TermsUrlField, string.Empty);
            txtCheckoutUrl.Text = paymentMethod.GetParameter(Constants.CheckoutUrlField, string.Empty);
            txtPushUrl.Text = paymentMethod.GetParameter(Constants.PushUrlField, string.Empty);
            txtNotificationUrl.Text = paymentMethod.GetParameter(Constants.NotificationUrlField, string.Empty);
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
            paymentMethod.SetParameter(Common.Constants.KlarnaUsernameField, txtUsername.Text);
            paymentMethod.SetParameter(Common.Constants.KlarnaPasswordField, txtPassword.Text);
            paymentMethod.SetParameter(Common.Constants.KlarnaApiUrlField, txtApiUrl.Text);
            
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorButtonField, txtColorButton.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorButtonTextField, txtColorButtonText.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorCheckboxField, txtColorCheckbox.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorHeaderField, txtColorHeader.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorLinkField, txtColorLink.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetRadiusBorderField, txtRadiusBorder.Text);
            paymentMethod.SetParameter(Constants.KlarnaWidgetColorCheckboxCheckmarkField, txtColorCheckboxCheckmark.Text);
            paymentMethod.SetParameter(Constants.AllowSeparateShippingAddressField, (allowSeparateShippingAddressCheckBox.Checked ? "true" : "false"));
            paymentMethod.SetParameter(Constants.DateOfBirthMandatoryField, (dateOfBirthMandatoryCheckBox.Checked ? "true" : "false"));
            paymentMethod.SetParameter(Constants.ShippingDetailsField, txtShippingDetails.Text);
            paymentMethod.SetParameter(Constants.TitleMandatoryField, (titleMandatoryCheckBox.Checked ? "true" : "false"));
            paymentMethod.SetParameter(Constants.ShowSubtotalDetailField, (showSubtotalDetailCheckBox.Checked ? "true" : "false"));
            paymentMethod.SetParameter(Constants.RequireValidateCallbackSuccessField, (requireValidateCallbackSuccessCheckBox.Checked ? "true" : "false"));
            paymentMethod.SetParameter(Constants.AdditionalCheckboxTextField, additionalCheckboxTextTextBox.Text);
            paymentMethod.SetParameter(Constants.AdditionalCheckboxDefaultCheckedField, (additionalCheckboxDefaultCheckedCheckBox.Checked ? "true" : "false"));
            paymentMethod.SetParameter(Constants.AdditionalCheckboxRequiredField, (additionalCheckboxRequiredCheckBox.Checked ? "true" : "false"));

            paymentMethod.SetParameter(Constants.ConfirmationUrlField, txtConfirmationUrl.Text);
            paymentMethod.SetParameter(Constants.TermsUrlField, txtTermsUrl.Text);
            paymentMethod.SetParameter(Constants.CheckoutUrlField, txtCheckoutUrl.Text);
            paymentMethod.SetParameter(Constants.PushUrlField, txtPushUrl.Text);
            paymentMethod.SetParameter(Constants.NotificationUrlField, txtNotificationUrl.Text);
        }
    }
}