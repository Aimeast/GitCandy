using GitCandy.Middlewares;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace GitCandy.Tests.Middlewares
{
    public class LightweightLocalizationMiddlewareTests
    {
        [Theory]
        [InlineData("", "", "en-US")]
        [InlineData("en", "", "en-US")]
        [InlineData("ZH-CN", "", "zh-CN")]
        [InlineData("", "fr-CH, fr;q=0.9, en;q=0.8, de;q=0.7, *;q=0.5", "fr-CH")] //developer.mozilla.org
        [InlineData("", "en-US,en;q=0.8,zh-CN;q=0.6,zh;q=0.4", "en-US")] //chrome
        [InlineData("", "da;q=0.6, en-gb;q=0.8, en;q=0.7", "en-GB")] //out of order
        [InlineData("", "*", "en-US")]
        [InlineData("en-gb", "en-US,en;q=0.5", "en-GB")]
        public async void Detect(string cookie, string acclang, string expected)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            var middleware = new LightweightLocalizationMiddleware(next => Task.CompletedTask);

            var httpContext = new DefaultHttpContext();
            if (!string.IsNullOrEmpty(cookie))
                httpContext.Request.Headers.Append("Cookie", "Lang=" + cookie);
            if (!string.IsNullOrEmpty(acclang))
                httpContext.Request.Headers.Append("Accept-Language", acclang);

            await middleware.Invoke(httpContext);

            Assert.Equal(expected, CultureInfo.CurrentCulture.Name);
            Assert.Equal(expected, CultureInfo.CurrentUICulture.Name);
        }
    }
}
