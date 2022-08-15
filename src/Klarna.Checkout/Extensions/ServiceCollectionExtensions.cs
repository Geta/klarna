using Klarna.Checkout;
using Klarna.OrderManagement.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains convenient extension methods to add Klarna Checkout to application
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Convenient method to add Klarna Checkout.
        /// </summary>
        /// <param name="services">the services.</param>
        /// <returns>The configured service container</returns>
        public static IServiceCollection AddKlarnaCheckout(this IServiceCollection services)
        {
            services.AddKlarnaOrderManagement();
            services.TryAddTransient<IKlarnaCartValidator, DefaultKlarnaCartValidator>();
            services.TryAddTransient<IKlarnaOrderValidator, KlarnaOrderValidator>();
            services.TryAddTransient<IKlarnaCheckoutService, KlarnaCheckoutService>();

            return services;
        }
    }
}