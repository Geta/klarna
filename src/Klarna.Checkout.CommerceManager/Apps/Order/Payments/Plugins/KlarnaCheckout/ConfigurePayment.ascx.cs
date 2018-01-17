using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ServiceLocation;
using Klarna.Common.Extensions;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;

namespace Klarna.Checkout.CommerceManager.Apps.Order.Payments.Plugins.KlarnaCheckout
{
    public partial class ConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        private PaymentMethodDto _paymentMethodDto;
        private IKlarnaCheckoutService _klarnaCheckoutService;

        public string ValidationGroup { get; set; }

        
        protected void Page_Load(object sender, EventArgs e)
        {
            _klarnaCheckoutService = ServiceLocator.Current.GetInstance<IKlarnaCheckoutService>();
            if (!base.IsPostBack && this._paymentMethodDto != null && this._paymentMethodDto.PaymentMethodParameter != null)
            {
                var markets = _paymentMethodDto.PaymentMethod.FirstOrDefault().GetMarketPaymentMethodsRows();

                if (markets != null)
                {
                    CheckoutConfiguration checkoutConfiguration = null;
                    try
                    {
                        checkoutConfiguration = _klarnaCheckoutService.GetConfiguration(markets.FirstOrDefault().MarketId);
                    }
                    catch
                    {
                        checkoutConfiguration = new CheckoutConfiguration();
                    }
                    BindData(checkoutConfiguration);

                    marketDropDownList.DataSource = markets.Select(m => m.MarketId);
                    marketDropDownList.DataBind();
                }
            }
        }

        public void LoadObject(object dto)
        {
            var paymentMethod = dto as PaymentMethodDto;

            if (paymentMethod == null)
            {
                return;
            }
            _paymentMethodDto = paymentMethod;
        }

        public void BindData(CheckoutConfiguration checkoutConfiguration)
        {
            txtUsername.Text = checkoutConfiguration.Username;
            txtPassword.Text = checkoutConfiguration.Password;
            txtApiUrl.Text = checkoutConfiguration.ApiUrl;

            txtColorButton.Text = checkoutConfiguration.WidgetButtonColor;
            txtColorButtonText.Text = checkoutConfiguration.WidgetButtonTextColor;
            txtColorCheckbox.Text = checkoutConfiguration.WidgetCheckboxColor;
            txtColorHeader.Text = checkoutConfiguration.WidgetHeaderColor;
            txtColorLink.Text = checkoutConfiguration.WidgetLinkColor;
            txtRadiusBorder.Text = checkoutConfiguration.WidgetBorderRadius;
            txtColorCheckboxCheckmark.Text = checkoutConfiguration.WidgetCheckboxCheckmarkColor;

            shippingOptionsInIFrameCheckBox.Checked = checkoutConfiguration.ShippingOptionsInIFrame;
            allowSeparateShippingAddressCheckBox.Checked = checkoutConfiguration.AllowSeparateShippingAddress;
            dateOfBirthMandatoryCheckBox.Checked = checkoutConfiguration.DateOfBirthMandatory;
            txtShippingDetails.Text = checkoutConfiguration.ShippingDetailsText;
            titleMandatoryCheckBox.Checked = checkoutConfiguration.TitleMandatory;
            showSubtotalDetailCheckBox.Checked = checkoutConfiguration.ShowSubtotalDetail;

            sendShippingCountriesCheckBox.Checked = checkoutConfiguration.SendShippingCountries;
            prefillAddressCheckBox.Checked = checkoutConfiguration.PrefillAddress;
            SendShippingOptionsPriorAddressesCheckBox.Checked = checkoutConfiguration.SendShippingOptionsPriorAddresses;
            SendProductAndImageUrlCheckBox.Checked = checkoutConfiguration.SendProductAndImageUrl;

            additionalCheckboxTextTextBox.Text = checkoutConfiguration.AdditionalCheckboxText;
            additionalCheckboxDefaultCheckedCheckBox.Checked = checkoutConfiguration.AdditionalCheckboxDefaultChecked;
            additionalCheckboxRequiredCheckBox.Checked = checkoutConfiguration.AdditionalCheckboxRequired;

            txtConfirmationUrl.Text = checkoutConfiguration.ConfirmationUrl;
            txtTermsUrl.Text = checkoutConfiguration.TermsUrl;
            txtCancellationTermsUrl.Text = checkoutConfiguration.CancellationTermsUrl;
            txtCheckoutUrl.Text = checkoutConfiguration.CheckoutUrl;
            txtPushUrl.Text = checkoutConfiguration.PushUrl;
            txtNotificationUrl.Text = checkoutConfiguration.NotificationUrl;
            txtShippingOptionUpdateUrl.Text = checkoutConfiguration.ShippingOptionUpdateUrl;
            txtAddressUpdateUrl.Text = checkoutConfiguration.AddressUpdateUrl;
            txtOrderValidationUrl.Text = checkoutConfiguration.OrderValidationUrl;
            requireValidateCallbackSuccessCheckBox.Checked = checkoutConfiguration.RequireValidateCallbackSuccess;
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
            var currentMarket = marketDropDownList.SelectedValue;

            var configuration = new CheckoutConfiguration();
            configuration.Username = txtUsername.Text;
            configuration.Password = txtPassword.Text;
            configuration.ApiUrl = txtApiUrl.Text;

            configuration.WidgetButtonColor = txtColorButton.Text;
            configuration.WidgetButtonTextColor = txtColorButtonText.Text;
            configuration.WidgetCheckboxColor = txtColorCheckbox.Text;
            configuration.WidgetHeaderColor = txtColorHeader.Text;
            configuration.WidgetLinkColor = txtColorLink.Text;
            configuration.WidgetBorderRadius = txtRadiusBorder.Text;
            configuration.WidgetCheckboxCheckmarkColor = txtColorCheckboxCheckmark.Text;
            configuration.ShippingOptionsInIFrame = shippingOptionsInIFrameCheckBox.Checked;
            configuration.AllowSeparateShippingAddress = allowSeparateShippingAddressCheckBox.Checked;
            configuration.DateOfBirthMandatory = dateOfBirthMandatoryCheckBox.Checked;
            configuration.ShippingDetailsText = txtShippingDetails.Text;
            configuration.TitleMandatory = titleMandatoryCheckBox.Checked;
            configuration.ShowSubtotalDetail = showSubtotalDetailCheckBox.Checked;

            configuration.SendShippingCountries = sendShippingCountriesCheckBox.Checked;
            configuration.PrefillAddress = prefillAddressCheckBox.Checked;
            configuration.SendShippingOptionsPriorAddresses = SendShippingOptionsPriorAddressesCheckBox.Checked;
            configuration.SendProductAndImageUrl = SendProductAndImageUrlCheckBox.Checked;

            configuration.AdditionalCheckboxText = additionalCheckboxTextTextBox.Text;
            configuration.AdditionalCheckboxDefaultChecked = additionalCheckboxDefaultCheckedCheckBox.Checked;
            configuration.AdditionalCheckboxRequired = additionalCheckboxRequiredCheckBox.Checked;

            configuration.ConfirmationUrl = txtConfirmationUrl.Text;
            configuration.TermsUrl = txtTermsUrl.Text;
            configuration.CancellationTermsUrl = txtCancellationTermsUrl.Text;
            configuration.CheckoutUrl = txtCheckoutUrl.Text;
            configuration.PushUrl = txtPushUrl.Text;
            configuration.NotificationUrl = txtNotificationUrl.Text;
            configuration.ShippingOptionUpdateUrl = txtShippingOptionUpdateUrl.Text;
            configuration.AddressUpdateUrl = txtAddressUpdateUrl.Text;
            configuration.OrderValidationUrl = txtOrderValidationUrl.Text;
            configuration.RequireValidateCallbackSuccess = requireValidateCallbackSuccessCheckBox.Checked;
            configuration.MarketId = currentMarket;

            paymentMethod.SetParameter($"{currentMarket}_{Common.Constants.KlarnaSerializedMarketOptions}", Newtonsoft.Json.JsonConvert.SerializeObject(configuration));
        }

        protected void marketDropDownList_OnSelectedIndexChanged(object sender, EventArgs e)
        {

            CheckoutConfiguration checkoutConfiguration = null;
            try
            {
                checkoutConfiguration = _klarnaCheckoutService.GetConfiguration(new MarketId(marketDropDownList.SelectedValue));
            }
            catch
            {
                checkoutConfiguration = new CheckoutConfiguration();
            }
            BindData(checkoutConfiguration);

            ConfigureUpdatePanelContentPanel.Update();
        }
    }
}