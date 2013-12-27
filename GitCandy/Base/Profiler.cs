using System;
using System.Diagnostics;
using System.Web;

namespace GitCandy.Base
{
    public class Profiler
    {
        const string CacheKey = "GitCandyProfiler";

        private readonly Stopwatch _sw;

        private Profiler()
        {
            _sw = new Stopwatch();
            _sw.Start();
        }

        public static Profiler Current
        {
            get
            {
                var context = HttpContext.Current;
                if (context == null) return null;

                return context.Items[CacheKey] as Profiler;
            }
            private set
            {
                var context = HttpContext.Current;
                if (context == null) return;

                context.Items[CacheKey] = value;
            }
        }

        public TimeSpan Elapsed { get { return _sw.Elapsed; } }

        public static void Start()
        {
            Current = new Profiler();
        }
    }
}