using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using ISO3166;
using Klarna.Common.Extensions;
using Klarna.Common.Helpers;
using Klarna.Payments.Extensions;
using Klarna.Payments.Helpers;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;

namespace Klarna.Payments.CommerceManager.Apps.Order.Payments.Plugins.KlarnaPayments
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

            txtKlarnaLogoUrl.Text = paymentMethod.GetParameter(Constants.KlarnaLogoUrlField, string.Empty);
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
        }

        public override void DataBind()
        {
            base.DataBind();

            var countries = CountryCodeHelper.GetCountryCodes().ToList();

            var preAssesmentCountries = _paymentMethodDto.GetParameter(Constants.PreAssesmentCountriesField, string.Empty)?.Split(',');

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
            paymentMethod.SetParameter(Common.Constants.KlarnaUsernameField, txtUsername.Text);
            paymentMethod.SetParameter(Common.Constants.KlarnaPasswordField, txtPassword.Text);
            paymentMethod.SetParameter(Common.Constants.KlarnaApiUrlField, txtApiUrl.Text);

            paymentMethod.SetParameter(Constants.KlarnaLogoUrlField, txtKlarnaLogoUrl.Text);
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

            var selectedCountries = new List<string>();
            
            foreach (var item in ltlSelector.GetSelectedItems())
            {
                selectedCountries.Add(item.Value);
            }
            paymentMethod.SetParameter(Constants.PreAssesmentCountriesField, string.Join(",", selectedCountries));
        }
    }
}