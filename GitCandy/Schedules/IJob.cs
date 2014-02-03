using System;

namespace GitCandy.Schedules
{
    public interface IJob
    {
        void Execute(JobContext jobContext);
        TimeSpan GetNextInterval(JobContext jobContext);
        TimeSpan Due { get; }
    }
}