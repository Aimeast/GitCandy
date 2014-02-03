using System;
using System.Collections.Generic;

namespace GitCandy.Schedules
{
    public sealed class Scheduler
    {
        private readonly static Scheduler _instance = new Scheduler();

        private readonly object _syncRoot = new object();
        private readonly LinkedList<JobContext> _jobs = new LinkedList<JobContext>();
        private readonly Runner _runner;

        public static Scheduler Instance { get { return _instance; } }

        public Scheduler()
        {
            _runner = new Runner(this);
        }

        public JobContext AddJob(IJob job)
        {
            var context = new JobContext(this, job);
            context.UtcCreation = DateTime.UtcNow;
            InsertJobContext(context);

            return context;
        }

        public void Start()
        {
            _runner.Start();
        }

        public void Stop()
        {
            _runner.Stop();
        }

        private void InsertJobContext(JobContext context)
        {
            if (context.ExecutionTimes == 0)
            {
                context.UtcNextExecution = DateTime.UtcNow.AddSeconds(1.0);
            }
            else
            {
                var interval = context.Job.GetNextInterval(context);
                if (interval == TimeSpan.MaxValue)
                    return;
                context.UtcNextExecution = DateTime.UtcNow.Add(interval);
            }

            lock (_syncRoot)
            {
                var node = _jobs.First;
                while (node != null)
                {
                    if ((node.Previous == null || context.UtcNextExecution <= context.UtcNextExecution)
                        && context.UtcNextExecution < node.Value.UtcNextExecution)
                    {
                        _jobs.AddBefore(node, context);
                        break;
                    }
                    node = node.Next;
                }
                if (node == null)
                    _jobs.AddLast(context);
            }
        }

        internal JobContext GetNextJobContext()
        {
            lock (_syncRoot)
            {
                var node = _jobs.First;
                if (node == null)
                    return null;

                var context = node.Value;
                if (DateTime.UtcNow < context.UtcNextExecution)
                    return null;

                _jobs.Remove(node);
                return context;
            }
        }

        internal void JobExecuted(JobContext context)
        {
            InsertJobContext(context);
        }
    }
}