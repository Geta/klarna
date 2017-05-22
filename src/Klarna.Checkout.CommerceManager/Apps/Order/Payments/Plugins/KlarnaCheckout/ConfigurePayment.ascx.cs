using System;
using System.Collections.Generic;
using System.Linq;
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
            var shippingOptionsInIFrame = bool.Parse(paymentMethod.GetParameter(Constants.ShippingOptionsInIFrameField, "true"));
            shippingOptionsInIFrameCheckBox.Checked = shippingOptionsInIFrame;
            var allowSeparateShippingAddress = bool.Parse(paymentMethod.GetParameter(Constants.AllowSeparateShippingAddressField, "false"));
            allowSeparateShippingAddressCheckBox.Checked = allowSeparateShippingAddress;
            var dateOfBirthMandatory = bool.Parse(paymentMethod.GetParameter(Constants.DateOfBirthMandatoryField, "false"));
            dateOfBirthMandatoryCheckBox.Checked = dateOfBirthMandatory;
            txtShippingDetails.Text = paymentMethod.GetParameter(Constants.ShippingDetailsField, string.Empty);
            var titleMandatory = bool.Parse(paymentMethod.GetParameter(Constants.TitleMandatoryField, "false"));
            titleMandatoryCheckBox.Checked = titleMandatory;
            var showSubtotalDetail = bool.Parse(paymentMethod.GetParameter(Constants.ShowSubtotalDetailField, "false"));
            showSubtotalDetailCheckBox.Checked = showSubtotalDetail;

            var sendShippingCountries = bool.Parse(paymentMethod.GetParameter(Constants.SendShippingCountriesField, "false"));
            sendShippingCountriesCheckBox.Checked = sendShippingCountries;
            var prefillAddress = bool.Parse(paymentMethod.GetParameter(Constants.PrefillAddressField, "false"));
            prefillAddressCheckBox.Checked = prefillAddress;
            var sendShippingOptionsPriorAddresses = bool.Parse(paymentMethod.GetParameter(Constants.SendShippingOptionsPriorAddressesField, "false"));
            SendShippingOptionsPriorAddressesCheckBox.Checked = sendShippingOptionsPriorAddresses;


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
            txtShippingOptionUpdateUrl.Text = paymentMethod.GetParameter(Constants.ShippingOptionUpdateUrlField, string.Empty);
            txtAddressUpdateUrl.Text = paymentMethod.GetParameter(Constants.AddressUpdateUrlField, string.Empty);
            txtOrderValidationUrl.Text = paymentMethod.GetParameter(Constants.OrderValidationUrlField, string.Empty);
            var requireValidateCallbackSuccess = bool.Parse(paymentMethod.GetParameter(Constants.RequireValidateCallbackSuccessField, "false"));
            requireValidateCallbackSuccessCheckBox.Checked = requireValidateCallbackSuccess;

            var markets = paymentMethod.PaymentMethod.FirstOrDefault().GetMarketPaymentMethodsRows();
            if (markets != null)
            {
                marketDropDownList.DataSource = markets.Select(m => m.MarketId);
            }
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
            var list = GetMarketConfigurations(paymentMethod);

            var configuration = new Configuration();
            configuration.AddressUpdateUrl = txtUsername.Text;
            configuration.AddressUpdateUrl = txtPassword.Text;
            configuration.AddressUpdateUrl = txtApiUrl.Text;

            configuration.AddressUpdateUrl = txtColorButton.Text;
            configuration.AddressUpdateUrl = txtColorButtonText.Text;
            configuration.AddressUpdateUrl = txtColorCheckbox.Text;
            configuration.AddressUpdateUrl = txtColorHeader.Text;
            configuration.AddressUpdateUrl = txtColorLink.Text;
            configuration.AddressUpdateUrl = txtRadiusBorder.Text;
            configuration.AddressUpdateUrl = txtColorCheckboxCheckmark.Text;
            configuration.ShippingOptionsInIFrame = shippingOptionsInIFrameCheckBox.Checked;
            configuration.AllowSeparateShippingAddress = allowSeparateShippingAddressCheckBox.Checked;
            configuration.DateOfBirthMandatory = dateOfBirthMandatoryCheckBox.Checked;
            configuration.ShippingDetailsText = txtShippingDetails.Text;
            configuration.TitleMandatory = titleMandatoryCheckBox.Checked;
            configuration.ShowSubtotalDetail = showSubtotalDetailCheckBox.Checked;

            configuration.SendShippingCountries = sendShippingCountriesCheckBox.Checked;
            configuration.PrefillAddress = prefillAddressCheckBox.Checked;
            configuration.SendShippingOptionsPriorAddresses = SendShippingOptionsPriorAddressesCheckBox.Checked;

            configuration.AdditionalCheckboxText = additionalCheckboxTextTextBox.Text;
            configuration.AdditionalCheckboxDefaultChecked = additionalCheckboxDefaultCheckedCheckBox.Checked;
            configuration.AdditionalCheckboxRequired = additionalCheckboxRequiredCheckBox.Checked;

            configuration.ConfirmationUrl = txtConfirmationUrl.Text;
            configuration.TermsUrl = txtTermsUrl.Text;
            configuration.CheckoutUrl = txtCheckoutUrl.Text;
            configuration.PushUrl = txtPushUrl.Text;
            configuration.NotificationUrl = txtNotificationUrl.Text;
            configuration.ShippingOptionUpdateUrl = txtShippingOptionUpdateUrl.Text;
            configuration.AddressUpdateUrl = txtAddressUpdateUrl.Text;
            configuration.OrderValidationUrl = txtOrderValidationUrl.Text;
            configuration.RequireValidateCallbackSuccess = requireValidateCallbackSuccessCheckBox.Checked;

            var currentConfiguration = list.FirstOrDefault(x => true);
            if (currentConfiguration != null)
            {
                currentConfiguration = configuration;
            }
            else
            {
                list.Add(configuration);
            }
            list.Add(configuration);

            paymentMethod.SetParameter(Constants.OrderValidationUrlField, Newtonsoft.Json.JsonConvert.SerializeObject(list));
        }

        private List<Configuration> GetMarketConfigurations(PaymentMethodDto paymentMethod)
        {
            var list = new List<Configuration>();
            var markets = paymentMethod.PaymentMethod.FirstOrDefault()?.GetMarketPaymentMethodsRows();
            //list.AddRange(markets.Select(m => ));

            return null;
            
        }

        protected void marketDropDownList_OnSelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}