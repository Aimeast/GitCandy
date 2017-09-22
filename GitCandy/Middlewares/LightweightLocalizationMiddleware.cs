using GitCandy.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GitCandy.Middlewares
{
    public class LightweightLocalizationMiddleware
    {
        private readonly RequestDelegate _next;

        public LightweightLocalizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            var langName = "en-us";
            if (httpContext.Request.Cookies["Lang"] is string cookie && !string.IsNullOrEmpty(cookie))
            {
                langName = cookie;
            }
            else if (httpContext.Request.GetTypedHeaders().AcceptLanguage
                ?.OrderByDescending(x => x.Quality ?? 1).First().Value.Value is string win
                && !string.IsNullOrEmpty(win)
                && win != "*")
            {
                langName = win;
            }

            CultureInfo culture;
            try
            {
                culture = CultureHelper.NameToCultureInfoCache(langName);
            }
            catch
            {
                culture = CultureHelper.NameToCultureInfoCache("en-us");
            }

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            return _next(httpContext);
        }
    }

    public static class LightweightLocalizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseLightweightLocalization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LightweightLocalizationMiddleware>();
        }
    }
}
