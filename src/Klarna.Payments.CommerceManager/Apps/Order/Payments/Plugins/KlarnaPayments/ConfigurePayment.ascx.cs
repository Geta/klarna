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
                    Configuration configuration = null;
                    try
                    {
                        configuration = _paymentMethodDto.GetConfiguration(markets.FirstOrDefault().MarketId);
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

        private void BindData(Configuration configuration)
        {
            txtUsername.Text = configuration.Username;
            txtPassword.Text = configuration.Password;
            txtApiUrl.Text = configuration.ApiUrl;

            txtKlarnaLogoUrl.Text = configuration.LogoUrl;
            txtColorDetails.Text = configuration.WidgetDetailsColor;
            txtColorButton.Text = configuration.WidgetButtonColor;
            txtColorButtonText.Text = configuration.WidgetButtonColor;
            txtColorCheckbox.Text = configuration.WidgetCheckboxColor;
            txtColorCheckboxCheckmark.Text = configuration.WidgetCheckboxCheckmarkColor;
            txtColorHeader.Text = configuration.WidgetHeaderColor;
            txtColorLink.Text = configuration.WidgetLinkColor;
            txtColorBorder.Text = configuration.WidgetBorderColor;
            txtColorBorderSelected.Text = configuration.WidgetSelectedBorderColor;
            txtColorText.Text = configuration.WidgetTextColor;
            txtColorTextSecondary.Text = configuration.WidgetTextSecondaryColor;
            txtRadiusBorder.Text = configuration.WidgetBorderRadius;

            txtConfirmationUrl.Text = configuration.ConfirmationUrl;
            txtNotificationUrl.Text = configuration.NotificationUrl;
            SendProductAndImageUrlCheckBox.Checked = configuration.SendProductAndImageUrlField;
            UseAttachmentsCheckBox.Checked = configuration.UseAttachments;
            PreAssesmentCheckBox.Checked = configuration.CustomerPreAssessment;
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

            Configuration configuration = null;
            try
            {
                configuration = _paymentMethodDto.GetConfiguration(marketDropDownList.SelectedValue);
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