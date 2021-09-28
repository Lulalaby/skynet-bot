using Microsoft.Extensions.DependencyInjection;
using Skynet.Wiki.API;

namespace Skynet.Wiki
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add all the things needed for the Wiki Service to work
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddWikiService(this IServiceCollection services)
        {
            services.AddSingleton<WikiApi>();
            
            return services;
        }
    }
}