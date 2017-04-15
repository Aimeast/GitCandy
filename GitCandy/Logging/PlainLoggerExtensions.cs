using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace GitCandy.Logging
{
    public static class PlainLoggerExtensions
    {
        public static ILoggerFactory AddPlain(this ILoggerFactory factory, IFileProvider root, bool includeScopes = false)
        {
            factory.AddProvider(new PlainLoggerProvider(root, includeScopes));
            return factory;
        }
    }
}
