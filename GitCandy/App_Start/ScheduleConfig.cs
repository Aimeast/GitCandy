using GitCandy.Schedules;
using System.Web.Mvc;

namespace GitCandy
{
    public static class ScheduleConfig
    {
        public static void RegisterScheduler()
        {
            var resolver = DependencyResolver.Current;
            var scheduler = Scheduler.Instance;

            foreach (var job in resolver.GetServices<IJob>())
            {
                scheduler.AddJob(job);
            }

            scheduler.Start();
        }
    }
}