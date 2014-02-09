using System;

namespace GitCandy.Schedules
{
    public sealed class JobContext
    {
        internal JobContext(Scheduler scheduler, IJob job)
        {
            Job = job;
            Scheduler = scheduler;
        }
        public Scheduler Scheduler { get; private set; }
        public IJob Job { get; private set; }

        public long ExecutionTimes { get; internal set; }
        public DateTime UtcCreation { get; internal set; }
        public DateTime? UtcStart { get; internal set; }
        public DateTime UtcLastStart { get; internal set; }
        public DateTime UtcLastEnd { get; internal set; }
        public DateTime UtcNextExecution { get; internal set; }
        public Exception LastException { get; internal set; }

        public string Name { get; set; }

        public event EventHandler<JobContext> Executing;
        public event EventHandler<JobContext> Executed;

        internal void OnExecuting(object sender, JobContext e)
        {
            if (Executing != null)
                Executing(sender, e);
        }
        internal void OnExecuted(object sender, JobContext e)
        {
            if (Executed != null)
                Executed(sender, e);
        }
    }
}