using Klarna.Common.Extensions;
using Klarna.OrderManagement.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klarna.OrderManagement.Extensions
{
    /// <summary>
    /// Contains convenient extension methods to add Klarna Order Management to application
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Convenient method to add Klarna Order Management.
        /// </summary>
        /// <param name="services">the services.</param>
        /// <returns>The configured service container</returns>
        public static IServiceCollection AddKlarnaOrderManagement(this IServiceCollection services)
        {
            services.AddKlarna();
            services.TryAddTransient<IKlarnaOrderServiceFactory, KlarnaOrderServiceFactory>();
            services.TryAddTransient<OrderCancelledEventHandler>();
            services.TryAddTransient<ReleaseRemainingEventHandler>();

            return services;
        }
    }
}