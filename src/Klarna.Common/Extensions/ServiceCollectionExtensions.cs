using Klarna.Common.Configuration;
using Klarna.Common.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klarna.Common.Extensions
{
    /// <summary>
    /// Contains convenient extension methods to add Klarna Common to application
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Convenient method to add Klarna.
        /// </summary>
        /// <param name="services">the services.</param>
        /// <returns>The configured service container</returns>
        public static IServiceCollection AddKlarna(this IServiceCollection services)
        {
            services.TryAddTransient<IConfigurationLoader, DefaultConfigurationLoader>();
            services.TryAddTransient<ILanguageService, DefaultLanguageService>();
            services.TryAddSingleton<ICountryRegionProvider, CountryRegionProvider>();

            return services;
        }
    }
}