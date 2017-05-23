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
                    Configuration configuration = null;
                    try
                    {
                        configuration = _klarnaCheckoutService.GetConfiguration(markets.FirstOrDefault().MarketId);
                    }
                    catch
                    {
                        configuration = new Configuration();
                    }
                    BindData(configuration);

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

        public void BindData(Configuration configuration)
        {
            txtUsername.Text = configuration.Username;
            txtPassword.Text = configuration.Password;
            txtApiUrl.Text = configuration.ApiUrl;

            txtColorButton.Text = configuration.WidgetButtonColor;
            txtColorButtonText.Text = configuration.WidgetButtonTextColor;
            txtColorCheckbox.Text = configuration.WidgetCheckboxColor;
            txtColorHeader.Text = configuration.WidgetHeaderColor;
            txtColorLink.Text = configuration.WidgetLinkColor;
            txtRadiusBorder.Text = configuration.WidgetBorderRadius;
            txtColorCheckboxCheckmark.Text = configuration.WidgetCheckboxCheckmarkColor;

            shippingOptionsInIFrameCheckBox.Checked = configuration.ShippingOptionsInIFrame;
            allowSeparateShippingAddressCheckBox.Checked = configuration.AllowSeparateShippingAddress;
            dateOfBirthMandatoryCheckBox.Checked = configuration.DateOfBirthMandatory;
            txtShippingDetails.Text = configuration.ShippingDetailsText;
            titleMandatoryCheckBox.Checked = configuration.TitleMandatory;
            showSubtotalDetailCheckBox.Checked = configuration.ShowSubtotalDetail;

            sendShippingCountriesCheckBox.Checked = configuration.SendShippingCountries;
            prefillAddressCheckBox.Checked = configuration.PrefillAddress;
            SendShippingOptionsPriorAddressesCheckBox.Checked = configuration.SendShippingOptionsPriorAddresses;


            additionalCheckboxTextTextBox.Text = configuration.AdditionalCheckboxText;
            additionalCheckboxDefaultCheckedCheckBox.Checked = configuration.AdditionalCheckboxDefaultChecked;
            additionalCheckboxRequiredCheckBox.Checked = configuration.AdditionalCheckboxRequired;

            txtConfirmationUrl.Text = configuration.ConfirmationUrl;
            txtTermsUrl.Text = configuration.TermsUrl;
            txtCheckoutUrl.Text = configuration.CheckoutUrl;
            txtPushUrl.Text = configuration.PushUrl;
            txtNotificationUrl.Text = configuration.NotificationUrl;
            txtShippingOptionUpdateUrl.Text = configuration.ShippingOptionUpdateUrl;
            txtAddressUpdateUrl.Text = configuration.AddressUpdateUrl;
            txtOrderValidationUrl.Text = configuration.OrderValidationUrl;
            requireValidateCallbackSuccessCheckBox.Checked = configuration.RequireValidateCallbackSuccess;
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

            var configuration = new Configuration();
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
            configuration.MarketId = currentMarket;

            paymentMethod.SetParameter($"{currentMarket}_{Common.Constants.KlarnaSerializedMarketOptions}", Newtonsoft.Json.JsonConvert.SerializeObject(configuration));
        }

        protected void marketDropDownList_OnSelectedIndexChanged(object sender, EventArgs e)
        {

            Configuration configuration = null;
            try
            {
                configuration = _klarnaCheckoutService.GetConfiguration(new MarketId(marketDropDownList.SelectedValue));
            }
            catch
            {
                configuration = new Configuration();
            }
            BindData(configuration);

            ConfigureUpdatePanelContentPanel.Update();
        }
    }
}