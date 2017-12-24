using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace GitCandy.Base
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Remove<T>(this IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(x => x.ServiceType == typeof(T));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            return services;
        }
    }
}
