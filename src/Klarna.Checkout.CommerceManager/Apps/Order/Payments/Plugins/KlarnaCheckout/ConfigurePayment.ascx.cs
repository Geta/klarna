using System;
using System.Linq;
using EPiServer.ServiceLocation;
using Klarna.Common.Extensions;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;
using Newtonsoft.Json;

namespace Klarna.Checkout.CommerceManager.Apps.Order.Payments.Plugins.KlarnaCheckout
{
    public partial class ConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        private PaymentMethodDto _paymentMethodDto;
        private ICheckoutConfigurationLoader _checkoutConfigurationLoader;

        public string ValidationGroup { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            _checkoutConfigurationLoader = ServiceLocator.Current.GetInstance<ICheckoutConfigurationLoader>();
            if (IsPostBack || _paymentMethodDto?.PaymentMethodParameter == null) return;

            var markets = _paymentMethodDto.PaymentMethod.First().GetMarketPaymentMethodsRows();
            if (markets == null || markets.Length == 0) return;

            var checkoutConfiguration = GetConfiguration(markets.First().MarketId);
            BindConfigurationData(checkoutConfiguration);
            BindMarketData(markets);
        }

        private void BindMarketData(PaymentMethodDto.MarketPaymentMethodsRow[] markets)
        {
            marketDropDownList.DataSource = markets.Select(m => m.MarketId);
            marketDropDownList.DataBind();
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

        public void BindConfigurationData(CheckoutConfiguration checkoutConfiguration)
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

            txtBaseUrl.Text = checkoutConfiguration.BaseUrl;
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

            var configuration = new CheckoutConfiguration
            {
                Username = txtUsername.Text,
                Password = txtPassword.Text,
                ApiUrl = txtApiUrl.Text,
                WidgetButtonColor = txtColorButton.Text,
                WidgetButtonTextColor = txtColorButtonText.Text,
                WidgetCheckboxColor = txtColorCheckbox.Text,
                WidgetHeaderColor = txtColorHeader.Text,
                WidgetLinkColor = txtColorLink.Text,
                WidgetBorderRadius = txtRadiusBorder.Text,
                WidgetCheckboxCheckmarkColor = txtColorCheckboxCheckmark.Text,
                ShippingOptionsInIFrame = shippingOptionsInIFrameCheckBox.Checked,
                AllowSeparateShippingAddress = allowSeparateShippingAddressCheckBox.Checked,
                DateOfBirthMandatory = dateOfBirthMandatoryCheckBox.Checked,
                ShippingDetailsText = txtShippingDetails.Text,
                TitleMandatory = titleMandatoryCheckBox.Checked,
                ShowSubtotalDetail = showSubtotalDetailCheckBox.Checked,
                SendShippingCountries = sendShippingCountriesCheckBox.Checked,
                PrefillAddress = prefillAddressCheckBox.Checked,
                SendShippingOptionsPriorAddresses = SendShippingOptionsPriorAddressesCheckBox.Checked,
                SendProductAndImageUrl = SendProductAndImageUrlCheckBox.Checked,
                AdditionalCheckboxText = additionalCheckboxTextTextBox.Text,
                AdditionalCheckboxDefaultChecked = additionalCheckboxDefaultCheckedCheckBox.Checked,
                AdditionalCheckboxRequired = additionalCheckboxRequiredCheckBox.Checked,
                BaseUrl = txtBaseUrl.Text,
                ConfirmationUrl = txtConfirmationUrl.Text,
                TermsUrl = txtTermsUrl.Text,
                CancellationTermsUrl = txtCancellationTermsUrl.Text,
                CheckoutUrl = txtCheckoutUrl.Text,
                PushUrl = txtPushUrl.Text,
                NotificationUrl = txtNotificationUrl.Text,
                ShippingOptionUpdateUrl = txtShippingOptionUpdateUrl.Text,
                AddressUpdateUrl = txtAddressUpdateUrl.Text,
                OrderValidationUrl = txtOrderValidationUrl.Text,
                RequireValidateCallbackSuccess = requireValidateCallbackSuccessCheckBox.Checked,
                MarketId = currentMarket
            };

            var serialized = JsonConvert.SerializeObject(configuration);
            paymentMethod.SetParameter($"{currentMarket}_{Common.Constants.KlarnaSerializedMarketOptions}", serialized);
        }

        protected void marketDropDownList_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var checkoutConfiguration = GetConfiguration(new MarketId(marketDropDownList.SelectedValue));
            BindConfigurationData(checkoutConfiguration);
            ConfigureUpdatePanelContentPanel.Update();
        }

        private CheckoutConfiguration GetConfiguration(MarketId marketId)
        {
            try
            {
                return _checkoutConfigurationLoader.GetConfiguration(marketId);
            }
            catch
            {
                return new CheckoutConfiguration();
            }
        }
    }
}