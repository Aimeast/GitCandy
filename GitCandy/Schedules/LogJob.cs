using GitCandy.Log;
using System;

namespace GitCandy.Schedules
{
    public class LogJob : IJob
    {
        public void Execute(JobContext jobContext)
        {
            if (jobContext.ExecutionTimes > 1)
                Logger.SetLogPath();
        }

        public TimeSpan GetNextInterval(JobContext jobContext)
        {
            return TimeSpan.FromSeconds(24 * 3600) - DateTime.Now.TimeOfDay;
        }

        public JobType JobType
        {
            get { return JobType.RealTime; }
        }
    }
}