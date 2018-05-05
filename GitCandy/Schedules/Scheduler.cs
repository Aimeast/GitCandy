using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace GitCandy.Schedules
{
    public sealed class Scheduler
    {
        private readonly Runner _onceQuicklyRunner;
        private readonly Runner _onceLonglyRunner;
        private readonly Runner _scheduledQuicklyRunner;
        private readonly Runner _scheduledLonglyRunner;

        public Scheduler(
            IApplicationLifetime applicationLifetime,
            ILoggerFactory loggerFactory)
        {
            _onceQuicklyRunner = new Runner(applicationLifetime.ApplicationStopping, loggerFactory, JobTypes.OnceQuickly, 4);
            _onceLonglyRunner = new Runner(applicationLifetime.ApplicationStopping, loggerFactory, JobTypes.OnceLongly, Environment.ProcessorCount);
            _scheduledQuicklyRunner = new Runner(applicationLifetime.ApplicationStopping, loggerFactory, JobTypes.ScheduledQuickly, 4);
            _scheduledLonglyRunner = new Runner(applicationLifetime.ApplicationStopping, loggerFactory, JobTypes.ScheduledLongly, Environment.ProcessorCount);
        }

        public void Start()
        {
            _onceQuicklyRunner.Start();
            _onceLonglyRunner.Start();
            _scheduledQuicklyRunner.Start();
            _scheduledLonglyRunner.Start();
        }

        public JobContext AddJob(IJob job, string jobName = null)
        {
            var context = new JobContext(job) { Name = jobName };
            switch (job.JobType)
            {
                case JobTypes.OnceQuickly:
                    _onceQuicklyRunner.Push(context);
                    break;
                case JobTypes.OnceLongly:
                    _onceLonglyRunner.Push(context);
                    break;
                case JobTypes.ScheduledQuickly:
                    _scheduledQuicklyRunner.Push(context);
                    break;
                case JobTypes.ScheduledLongly:
                    _scheduledLonglyRunner.Push(context);
                    break;
                default:
                    throw new NotSupportedException($"The {job.JobType} job \"{context.Name}\" not supported");
            }

            return context;
        }
    }
}
