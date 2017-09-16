using GitCandy.Middlewares;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace GitCandy.Tests.Middlewares
{
    public class ProfilerMiddlewareTests
    {
        [Fact]
        public async void AlreadySetup()
        {
            var httpContext = new DefaultHttpContext();

            var sw = httpContext.Features.Get<Stopwatch>();
            Assert.Null(sw);

            var middleware = new ProfilerMiddleware(next => Task.CompletedTask);

            await middleware.Invoke(httpContext);

            sw = httpContext.Features.Get<Stopwatch>();
            Assert.NotNull(sw);
            Assert.True(sw.IsRunning);
            Assert.True(sw.Elapsed > TimeSpan.Zero);
        }
    }
}
