using EPiServer.ServiceLocation;
using Klarna.Common.Extensions;
using Mediachase.Commerce.Orders.Dto;

namespace Klarna.Common
{
    [ServiceConfiguration(typeof(IConnectionFactory))]
    public class KlarnaConnectionFactory : IConnectionFactory
    {
        public ConnectionConfiguration GetConnectionConfiguration(PaymentMethodDto paymentMethod)
        {
            var username = paymentMethod.GetParameter(Constants.KlarnaUsernameField, string.Empty);
            var password = paymentMethod.GetParameter(Constants.KlarnaPasswordField, string.Empty);
            var apiUrl = paymentMethod.GetParameter(Constants.KlarnaApiUrlField, string.Empty);

            return new ConnectionConfiguration
            {
                ApiUrl = apiUrl,
                Username = username,
                Password = password
            };
        }
    }
}
