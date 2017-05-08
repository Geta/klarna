using EPiServer.ServiceLocation;
using Klarna.Common;
using Klarna.Common.Extensions;
using Mediachase.Commerce.Orders.Dto;

namespace Klarna.Payments
{
    [ServiceConfiguration(typeof(ConnectionFactory))]
    public class PaymentsConnectionFactory : ConnectionFactory
    {
        public override Common.ConnectionConfiguration GetConnectionConfiguration(PaymentMethodDto paymentMethod)
        {
            var username = paymentMethod.GetParameter(Common.Constants.KlarnaUsernameField, string.Empty);
            var password = paymentMethod.GetParameter(Common.Constants.KlarnaPasswordField, string.Empty);
            var apiUrl = paymentMethod.GetParameter(Common.Constants.KlarnaApiUrlField, string.Empty);

            return new Common.ConnectionConfiguration
            {
                ApiUrl = apiUrl,
                Username = username,
                Password = password
            };
        }
    }
}
