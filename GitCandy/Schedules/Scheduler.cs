using GitCandy.Log;
using System;
using System.Collections.Generic;

namespace GitCandy.Schedules
{
    public sealed class Scheduler
    {
        const double RealTimeDuration = 3.0;

        private readonly static Scheduler _instance = new Scheduler();

        private readonly object _syncRoot = new object();
        private readonly LinkedList<JobContext> _jobs = new LinkedList<JobContext>();
        private readonly Runner[] _runners;

        public static Scheduler Instance { get { return _instance; } }

        public Scheduler()
        {
            _runners = new Runner[Environment.ProcessorCount * 2];
            var div = Math.Min(_runners.Length / 4, 3);
            for (int i = 0; i < _runners.Length; i++)
            {
                _runners[i] = new Runner(this, i < div ? RunnerType.RealTime : RunnerType.LongRunning);
            }
        }

        public JobContext AddJob(IJob job, string jobName = null)
        {
            var context = new JobContext(this, job) { Name = jobName };
            context.UtcCreation = DateTime.UtcNow;
            InsertJobContext(context);

            return context;
        }

        public void StartAll()
        {
            for (int i = 0; i < _runners.Length; i++)
            {
                _runners[i].Start();
                Logger.Info("Schedule runner #{0} {1} started", _runners[i].ID, _runners[i].RunnerType);
            }
        }

        public void StopAll()
        {
            for (int i = 0; i < _runners.Length; i++)
            {
                Logger.Info("Schedule runner #{0} stopping", _runners[i].ID);
                _runners[i].Stop();
            }
        }

        public void WaitAll()
        {
            for (int i = 0; i < _runners.Length; i++)
            {
                _runners[i].WaitForExit(10 * 1000);
            }
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

        internal JobContext GetNextJobContext(RunnerType runnerType)
        {
            lock (_syncRoot)
            {
                var node = _jobs.First;
                while (node != null)
                {
                    var context = node.Value;
                    var due = context.Job.Due.Seconds;
                    if (DateTime.UtcNow >= context.UtcNextExecution
                        && (runnerType == RunnerType.RealTime && due < RealTimeDuration
                            || runnerType == RunnerType.LongRunning))
                    {
                        _jobs.Remove(node);
                        return context;
                    }

                    node = node.Next;
                }
                return null;
            }
        }

        internal void JobExecuted(JobContext context)
        {
            InsertJobContext(context);
        }
    }
}