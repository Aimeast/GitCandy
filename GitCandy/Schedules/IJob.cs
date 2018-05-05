using System;

namespace GitCandy.Schedules
{
    public interface IJob
    {
        void Execute(JobContext jobContext);
        DateTime GetNextExecutionTime(JobContext jobContext);
        JobTypes JobType { get; }
    }
}
