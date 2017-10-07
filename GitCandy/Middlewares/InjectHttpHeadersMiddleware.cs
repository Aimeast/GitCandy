using GitCandy.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GitCandy.Middlewares
{
    public class InjectHttpHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public InjectHttpHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.Headers.Add("X-GitCandy-Version", AppInformation.Version.ToString());

            return _next(httpContext);
        }
    }

    public static class InjectHttpHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseInjectHttpHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<InjectHttpHeadersMiddleware>();
        }
    }
}
