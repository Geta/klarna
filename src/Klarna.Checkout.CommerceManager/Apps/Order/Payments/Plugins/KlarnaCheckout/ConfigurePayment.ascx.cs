using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ServiceLocation;
using Klarna.Common.Extensions;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;

namespace Klarna.Checkout.CommerceManager.Apps.Order.Payments.Plugins.KlarnaCheckout
{
    public partial class ConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        private PaymentMethodDto _paymentMethodDto;
        private Injected<IKlarnaCheckoutService> _klarnaCheckoutService; 

        public string ValidationGroup { get; set; }

        public void LoadObject(object dto)
        {
            var paymentMethod = dto as PaymentMethodDto;

            if (paymentMethod == null)
            {
                return;
            }
            _paymentMethodDto = paymentMethod;

            var markets = paymentMethod.PaymentMethod.FirstOrDefault().GetMarketPaymentMethodsRows();

            if(markets != null)
            {     
                Configuration configuration = null;
                try
                {
                    if (IsPostBack)
                    {
                        configuration = _klarnaCheckoutService.Service.GetConfiguration(marketDropDownList.SelectedValue);
                    }
                    else
                    {
                        configuration =
                            _klarnaCheckoutService.Service.GetConfiguration(markets.FirstOrDefault().MarketId);
                    }
                }
                catch
                {
                    configuration = new Configuration();
                }

                txtUsername.Text = configuration.AddressUpdateUrl;
                txtPassword.Text = configuration.AddressUpdateUrl;
                txtApiUrl.Text = configuration.AddressUpdateUrl;

                txtColorButton.Text = configuration.AddressUpdateUrl;
                txtColorButtonText.Text = configuration.AddressUpdateUrl;
                txtColorCheckbox.Text = configuration.AddressUpdateUrl;
                txtColorHeader.Text = configuration.AddressUpdateUrl;
                txtColorLink.Text = configuration.AddressUpdateUrl;
                txtRadiusBorder.Text = configuration.AddressUpdateUrl;
                txtColorCheckboxCheckmark.Text = configuration.AddressUpdateUrl;
                
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
            var currentMarket = marketDropDownList.SelectedValue;

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
            configuration.MarketId = currentMarket;

            var currentConfiguration = list.FirstOrDefault(x => x.MarketId == currentMarket);
            if (currentConfiguration != null)
            {
                currentConfiguration = configuration;
            }
            else
            {
                list.Add(configuration);
            }
            paymentMethod.SetParameter(Common.Constants.KlarnaSerializedMarketOptions, Newtonsoft.Json.JsonConvert.SerializeObject(list));
        }

        private List<Configuration> GetMarketConfigurations(PaymentMethodDto paymentMethod)
        {
            var list = new List<Configuration>();
            var markets = paymentMethod.PaymentMethod.FirstOrDefault()?.GetMarketPaymentMethodsRows();
            if (markets != null)
            {
                foreach (var item in markets)
                {
                    try
                    {
                        list.Add(_klarnaCheckoutService.Service.GetConfiguration(item.MarketId));
                    }
                    catch { }
                }
            }
            return list;
        }

        protected void marketDropDownList_OnSelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}