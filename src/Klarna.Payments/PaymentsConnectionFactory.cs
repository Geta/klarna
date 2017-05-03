using EPiServer.ServiceLocation;
using Klarna.Common;
using Klarna.Payments.Extensions;
using Mediachase.Commerce.Orders.Dto;

namespace Klarna.Payments
{
    [ServiceConfiguration(typeof(ConnectionFactory))]
    public class PaymentsConnectionFactory : ConnectionFactory
    {
        public override Common.ConnectionConfiguration GetConnectionConfiguration(PaymentMethodDto paymentMethod)
        {
            var username = paymentMethod.GetParameter(Constants.KlarnaUsernameField, string.Empty);
            var password = paymentMethod.GetParameter(Constants.KlarnaPasswordField, string.Empty);
            var apiUrl = paymentMethod.GetParameter(Constants.KlarnaApiUrlField, string.Empty);

            return new Common.ConnectionConfiguration
            {
                ApiUrl = apiUrl,
                Username = username,
                Password = password
            };
        }
    }
}
