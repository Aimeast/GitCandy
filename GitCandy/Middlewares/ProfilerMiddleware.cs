using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GitCandy.Middlewares
{
    public class ProfilerMiddleware
    {
        private readonly RequestDelegate _next;

        public ProfilerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            httpContext.Features.Set(Stopwatch.StartNew());

            return _next(httpContext);
        }
    }

    public static class ProfilerMiddlewareExtensions
    {
        public static IApplicationBuilder UseProfiler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ProfilerMiddleware>();
        }
    }
}
