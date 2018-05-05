using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace GitCandy.Schedules
{
    public sealed class Runner
    {
        private readonly static int IntervalOfTask = 500;
        private readonly static TimeSpan WarnElapsed = TimeSpan.FromSeconds(2);

        private readonly object _syncRoot = new object();
        private readonly CancellationToken _cancellationToken;
        private readonly ILogger _logger;
        private readonly LinkedList<JobContext> _jobs = new LinkedList<JobContext>();
        private readonly Task[] _tasks;

        public Runner(
            CancellationToken cancellationToken,
            ILoggerFactory loggerFactory,
            JobTypes jobType,
            int count)
        {
            JobType = jobType;
            _cancellationToken = cancellationToken;
            _logger = loggerFactory.CreateLogger<Runner>();
            _tasks = new Task[count];
        }

        public JobTypes JobType { get; private set; }

        public void Start()
        {
            for (int i = 0; i < _tasks.Length; i++)
            {
                _tasks[i] = Task.Run(() => RunnerLoop(i));
            }
        }

        public void Push(JobContext context)
        {
            Contract.Assert(context.Job.JobType == JobType);

            if (context.Job.JobType == JobTypes.OnceQuickly || context.Job.JobType == JobTypes.OnceLongly)
            {
                if (context.ExecutedTimes == 0)
                {
                    lock (_jobs)
                    {
                        _jobs.AddLast(context);
                    }
                }
            }
            else if (context.Job.JobType == JobTypes.ScheduledQuickly || context.Job.JobType == JobTypes.ScheduledLongly)
            {
                var nextTime = context.Job.GetNextExecutionTime(context);
                if (nextTime >= DateTime.Now)
                {
                    lock (_jobs)
                    {
                        var node = _jobs.First;
                        while (node != null && node.Value.Job.GetNextExecutionTime(node.Value) <= nextTime)
                        {
                            node = node.Next;
                        }
                        if (node == null)
                            _jobs.AddLast(context);
                        else
                            _jobs.AddBefore(node, context);
                    }
                }
                else
                {
                    _logger.LogWarning($"The {context.Job.JobType} job \"{context.Name}\" exceed now datetime, to be discarded.");
                }
            }
            else
            {
                _logger.LogWarning($"The {context.Job.JobType} job \"{context.Name}\" not supported.");
            }
        }

        public JobContext GetNextContext()
        {
            var first = _jobs.First;
            if (first != null)
            {
                lock (_jobs)
                {
                    first = _jobs.First;
                    if (first != null)
                    {
                        if (first.Value.Job.JobType == JobTypes.OnceLongly
                            || first.Value.Job.JobType == JobTypes.OnceQuickly
                            || first.Value.Job.GetNextExecutionTime(first.Value) <= DateTime.Now)
                        {
                            _jobs.RemoveFirst();
                            return first.Value;
                        }
                    }
                }
            }
            return null;
        }

        private void RunnerLoop(int runnerID)
        {
            while (true)
            {
                if (_cancellationToken.IsCancellationRequested)
                    break;

                var context = GetNextContext();
                if (context != null)
                {
                    var jobName = context.Job.GetType().FullName;
                    if (!string.IsNullOrEmpty(context.Name))
                        jobName += " (" + context.Name + ")";
                    try
                    {
                        context.ExecutedTimes++;
                        _logger.LogInformation($"{JobType} {jobName} executing on runner #{runnerID}");
                        context.LastStarting = DateTime.Now;
                        context.Job.Execute(context);
                        context.LastEnding = DateTime.Now;
                        var elapsed = context.LastStarting - context.LastEnding;

                        if (JobType != JobTypes.OnceLongly && elapsed > WarnElapsed)
                            _logger.LogWarning($"{JobType} {jobName} executed on runner #{runnerID}, elapsed {elapsed}");
                        else
                            _logger.LogInformation($"{JobType} {jobName} executed on runner #{runnerID}, elapsed {elapsed}");

                        context.LastException = null;
                    }
                    catch (Exception ex)
                    {
                        context.LastException = ex;
                        _logger.LogError($"{JobType} {jobName} exception on runner #{runnerID}" + Environment.NewLine + ex);
                    }

                    Push(context);
                }
                else
                {
                    Task.Delay(IntervalOfTask).Wait();
                }
            }

            _logger.LogInformation($"Exit {JobType} runner #{runnerID} loop, {_jobs.Count} jobs left over");
        }
    }
}
