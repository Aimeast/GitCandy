using System;

namespace GitCandy.Schedules
{
    public sealed class JobContext
    {
        public JobContext(IJob job)
        {
            Job = job;
        }
        public IJob Job { get; private set; }

        public long ExecutedTimes { get; set; }
        public DateTime LastStarting { get; set; }
        public DateTime LastEnding { get; set; }
        public Exception LastException { get; set; }

        public string Name { get; set; }
    }
}
