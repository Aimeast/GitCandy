using Microsoft.Extensions.DependencyInjection;

namespace GitCandy.Accessories
{
    public static class AccessoriesExtensions
    {
        public static IServiceCollection AddAccessories(this IServiceCollection services)
        {
            services
                .AddSingleton<IProfilerAccessor, ProfilerAccessor>()
                .AddSingleton<ITokenAccessor, TokenAccessor>()
                ;

            return services;
        }
    }
}
