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

            //txtConfirmationUrl.Text = configuration.conf
            //txtNotificationUrl.Text = confi paymentMethod.GetParameter(Constants.NotificationUrlField, string.Empty);
            SendProductAndImageUrlCheckBox.Checked = configuration.SendProductAndImageUrlField;
            UseAttachmentsCheckBox.Checked = configuration.UseAttachments;
        }

        public override void DataBind()
        {
            base.DataBind();

            var countries = CountryCodeHelper.GetCountryCodes().ToList();

            Configuration configuration = null;
            try
            {
                if (!IsPostBack)
                {
                    var markets = _paymentMethodDto.PaymentMethod.FirstOrDefault().GetMarketPaymentMethodsRows();
                    configuration = _paymentMethodDto.GetConfiguration(markets.FirstOrDefault().MarketId);
                }
                else
                {
                    configuration = _paymentMethodDto.GetConfiguration(marketDropDownList.SelectedValue);
                }
            }
            catch
            {
                configuration = new Configuration();
            }

            var preAssesmentCountries = configuration.CustomerPreAssessmentCountries;

            var selectedCountries = countries.Where(x => preAssesmentCountries.Any(c => c == x.ThreeLetterCode)).OrderBy(x => x.Name).ToList();

            this.BindSourceGrid(countries.Where(c => selectedCountries.All(x => x.ThreeLetterCode != c.ThreeLetterCode)));
            this.BindTargetGrid(selectedCountries);
        }

        private void BindSourceGrid(IEnumerable<Country> countries)
        {
            this.ltlSelector.ClearSourceItems();
            this.lbSource.Items.Clear();
            this.lbSource.DataSource = countries;
            this.lbSource.DataBind();

            foreach (Country country in countries)
            {
                ListItem listItem = this.lbSource.Items.FindByValue(country.ThreeLetterCode);
                if (listItem == null)
                {
                    continue;
                }
                this.ltlSelector.Items.Add(listItem);
            }
        }

        private void BindTargetGrid(IEnumerable<Country> countries)
        {
            this.ltlSelector.ClearTargetItems();
            this.lbTarget.Items.Clear();

            if (countries.Any())
            {
                this.lbTarget.Items.Clear();
                foreach (Country country in countries)
                {
                    ListItem listItem = new ListItem(country.Name, country.ThreeLetterCode, true);
                    this.lbTarget.Items.Add(listItem);
                    listItem.Selected = true;
                    this.ltlSelector.Items.Add(listItem);
                }
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

            //configuration.WidgetBorderRadius = txtConfirmationUrl.Text;
            //configuration.WidgetBorderRadius = txtNotificationUrl.Text;
            configuration.SendProductAndImageUrlField = SendProductAndImageUrlCheckBox.Checked;
            configuration.UseAttachments = UseAttachmentsCheckBox.Checked;

            var selectedCountries = new List<string>();
            
            foreach (var item in ltlSelector.GetSelectedItems())
            {
                selectedCountries.Add(item.Value);
            }
            configuration.CustomerPreAssessmentCountries = selectedCountries;

            var currentConfiguration = list.FirstOrDefault(x => x.MarketId == currentMarket);
            if (currentConfiguration != null)
            {
                list.Remove(currentConfiguration);
            }
            list.Add(configuration);

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
                        list.Add(paymentMethod.GetConfiguration(item.MarketId));
                    }
                    catch { }
                }
            }
            return list;
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