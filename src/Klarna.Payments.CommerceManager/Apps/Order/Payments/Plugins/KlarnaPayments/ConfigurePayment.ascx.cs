using Klarna.Payments.Extensions;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;

namespace Klarna.Payments.CommerceManager.Apps.Order.Payments.Plugins.KlarnaPayments
{
    public partial class ConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        public string ValidationGroup { get; set; }

        public void LoadObject(object dto)
        {
            var paymentMethod = dto as PaymentMethodDto;

            if (paymentMethod == null)
            {
                return;
            }
            txtUsername.Text = paymentMethod.GetParameter(Constants.KlarnaUsernameField, string.Empty);
            txtPassword.Text = paymentMethod.GetParameter(Constants.KlarnaPasswordField, string.Empty);
            var isProduction = bool.Parse(paymentMethod.GetParameter(Constants.KlarnaIsProductionField, "false"));
            IsProductionCheckBox.Checked = isProduction;
            
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
            paymentMethod.SetParameter(Constants.KlarnaUsernameField, txtUsername.Text);
            paymentMethod.SetParameter(Constants.KlarnaPasswordField, txtPassword.Text);
            paymentMethod.SetParameter(Constants.KlarnaIsProductionField, (IsProductionCheckBox.Checked ? "true" : "false"));
        }
        
    }
}