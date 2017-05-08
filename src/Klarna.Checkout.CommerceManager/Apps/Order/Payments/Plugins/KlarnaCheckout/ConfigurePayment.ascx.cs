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

            txtUsername.Text = paymentMethod.GetParameter("", string.Empty);
            txtPassword.Text = paymentMethod.GetParameter("", string.Empty);
            txtApiUrl.Text = paymentMethod.GetParameter("", string.Empty);
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
            paymentMethod.SetParameter("", txtUsername.Text);
            paymentMethod.SetParameter("", txtPassword.Text);
            paymentMethod.SetParameter("", txtApiUrl.Text);
        }
    }
}