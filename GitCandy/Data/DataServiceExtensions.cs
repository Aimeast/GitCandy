using Microsoft.Extensions.DependencyInjection;

namespace GitCandy.Data
{
    public static class DataServiceExtensions
    {
        public static IServiceCollection AddDataService(this IServiceCollection services, DataServiceSettings settings)
        {
            services
                .AddSingleton(settings)
                .AddSingleton<DataService>();

            return services;
        }
    }
}
