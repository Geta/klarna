using Klarna.OrderManagement.Extensions;
using Klarna.Payments;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains convenient extension methods to add Klarna Payments to application
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Convenient method to add Klarna Payments.
        /// </summary>
        /// <param name="services">the services.</param>
        /// <returns>The configured service container</returns>
        public static IServiceCollection AddKlarnaPayments(this IServiceCollection services)
        {
            services.AddKlarnaOrderManagement();
            services.TryAddTransient<IKlarnaPaymentsService, KlarnaPaymentsService>();

            return services;
        }
    }
}