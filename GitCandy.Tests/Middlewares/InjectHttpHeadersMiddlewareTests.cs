using GitCandy.Base;
using GitCandy.Middlewares;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Xunit;

namespace GitCandy.Tests.Middlewares
{
    public class InjectHttpHeadersMiddlewareTests
    {
        [Fact]
        public async void AlreadySetup()
        {
            var httpContext = new DefaultHttpContext();
            var middleware = new InjectHttpHeadersMiddleware(next => Task.CompletedTask);

            await middleware.Invoke(httpContext);

            var version = httpContext.Response.Headers["X-GitCandy-Version"];

            Assert.Equal(AppInformation.Version.ToString(), version);
        }
    }
}
