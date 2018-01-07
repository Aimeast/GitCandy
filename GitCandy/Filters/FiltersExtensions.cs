using Microsoft.Extensions.DependencyInjection;

namespace GitCandy.Filters
{
    public static class FiltersExtensions
    {
        public static IServiceCollection AddFilters(this IServiceCollection services)
        {
            services
                .AddSingleton<TokenFilter>()
                .AddSingleton<AllowRegisterUserFilter>()
                ;

            return services;
        }
    }
}
