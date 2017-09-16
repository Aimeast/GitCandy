using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;

namespace GitCandy.Accessories
{
    public class ProfilerAccessor : IProfilerAccessor
    {
        private IHttpContextAccessor _httpContextAccessor;

        public ProfilerAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public TimeSpan Elapsed => _httpContextAccessor.HttpContext.Features.Get<Stopwatch>()?.Elapsed ?? TimeSpan.Zero;
    }

    public interface IProfilerAccessor
    {
        TimeSpan Elapsed { get; }
    }
}
