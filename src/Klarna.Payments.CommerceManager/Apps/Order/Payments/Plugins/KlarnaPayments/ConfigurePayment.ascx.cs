using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using EPiServer.ServiceLocation;
using ISO3166;
using Klarna.Common.Extensions;
using Klarna.Common.Helpers;
using Klarna.Payments.Extensions;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;

namespace Klarna.Payments.CommerceManager.Apps.Order.Payments.Plugins.KlarnaPayments
{
    public partial class ConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        private PaymentMethodDto _paymentMethodDto;
        private Injected<IKlarnaPaymentsService> _klarnaPaymentService;

        public string ValidationGroup { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!base.IsPostBack && this._paymentMethodDto != null && this._paymentMethodDto.PaymentMethodParameter != null)
            {
                var markets = _paymentMethodDto.PaymentMethod.FirstOrDefault().GetMarketPaymentMethodsRows();

                if (markets != null)
                {
                    PaymentsConfiguration paymentsConfiguration = null;
                    try
                    {
                        paymentsConfiguration = _paymentMethodDto.GetKlarnaPaymentsConfiguration(markets.FirstOrDefault().MarketId);
                    }
                    catch
                    {
                        paymentsConfiguration = new PaymentsConfiguration();
                    }
                    BindData(paymentsConfiguration);

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

        private void BindData(PaymentsConfiguration paymentsConfiguration)
        {
            txtUsername.Text = paymentsConfiguration.Username;
            txtPassword.Text = paymentsConfiguration.Password;
            txtApiUrl.Text = paymentsConfiguration.ApiUrl;

            txtKlarnaLogoUrl.Text = paymentsConfiguration.LogoUrl;
            txtColorDetails.Text = paymentsConfiguration.WidgetDetailsColor;
            txtColorButton.Text = paymentsConfiguration.WidgetButtonColor;
            txtColorButtonText.Text = paymentsConfiguration.WidgetButtonColor;
            txtColorCheckbox.Text = paymentsConfiguration.WidgetCheckboxColor;
            txtColorCheckboxCheckmark.Text = paymentsConfiguration.WidgetCheckboxCheckmarkColor;
            txtColorHeader.Text = paymentsConfiguration.WidgetHeaderColor;
            txtColorLink.Text = paymentsConfiguration.WidgetLinkColor;
            txtColorBorder.Text = paymentsConfiguration.WidgetBorderColor;
            txtColorBorderSelected.Text = paymentsConfiguration.WidgetSelectedBorderColor;
            txtColorText.Text = paymentsConfiguration.WidgetTextColor;
            txtColorTextSecondary.Text = paymentsConfiguration.WidgetTextSecondaryColor;
            txtRadiusBorder.Text = paymentsConfiguration.WidgetBorderRadius;

            txtConfirmationUrl.Text = paymentsConfiguration.ConfirmationUrl;
            txtNotificationUrl.Text = paymentsConfiguration.NotificationUrl;
            SendProductAndImageUrlCheckBox.Checked = paymentsConfiguration.SendProductAndImageUrlField;
            UseAttachmentsCheckBox.Checked = paymentsConfiguration.UseAttachments;
            PreAssesmentCheckBox.Checked = paymentsConfiguration.CustomerPreAssessment;
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

            var configuration = new PaymentsConfiguration();
            configuration.MarketId = currentMarket;

            configuration.Username = txtUsername.Text;
            configuration.Password = txtPassword.Text;
            configuration.ApiUrl = txtApiUrl.Text;

            configuration.LogoUrl = txtKlarnaLogoUrl.Text;
            configuration.WidgetDetailsColor = txtColorDetails.Text;
            configuration.WidgetButtonColor = txtColorButton.Text;
            configuration.WidgetButtonTextColor = txtColorButtonText.Text;
            configuration.WidgetCheckboxColor = txtColorCheckbox.Text;
            configuration.WidgetCheckboxCheckmarkColor = txtColorCheckboxCheckmark.Text;
            configuration.WidgetHeaderColor = txtColorHeader.Text;
            configuration.WidgetLinkColor = txtColorLink.Text;
            configuration.WidgetBorderColor = txtColorBorder.Text;
            configuration.WidgetSelectedBorderColor = txtColorBorderSelected.Text;
            configuration.WidgetTextColor = txtColorText.Text;
            configuration.WidgetTextSecondaryColor = txtColorTextSecondary.Text;
            configuration.WidgetBorderRadius = txtRadiusBorder.Text;

            configuration.ConfirmationUrl = txtConfirmationUrl.Text;
            configuration.NotificationUrl = txtNotificationUrl.Text;
            configuration.SendProductAndImageUrlField = SendProductAndImageUrlCheckBox.Checked;
            configuration.UseAttachments = UseAttachmentsCheckBox.Checked;
            configuration.CustomerPreAssessment = PreAssesmentCheckBox.Checked;

            paymentMethod.SetParameter($"{currentMarket}_{Common.Constants.KlarnaSerializedMarketOptions}", Newtonsoft.Json.JsonConvert.SerializeObject(configuration));
        }

        protected void marketDropDownList_OnSelectedIndexChanged(object sender, EventArgs e)
        {

            PaymentsConfiguration paymentsConfiguration = null;
            try
            {
                paymentsConfiguration = _paymentMethodDto.GetKlarnaPaymentsConfiguration(marketDropDownList.SelectedValue);
            }
            catch
            {
                paymentsConfiguration = new PaymentsConfiguration();
            }
            BindData(paymentsConfiguration);

            ConfigureUpdatePanelContentPanel.Update();
        }
    }
}