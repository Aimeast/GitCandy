﻿using GitCandy.Log;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GitCandy.Schedules
{
    internal sealed class Runner
    {
        private readonly static int IntervalOfTask = 100;

        private readonly object _syncRoot = new object();
        private readonly Scheduler _scheduler;

        private static int _id = 0;

        private CancellationTokenSource _tokenSource;
        private Task _task;

        public Runner(Scheduler scheduler, JobType jobType)
        {
            _scheduler = scheduler;
            JobType = jobType;
            ID = Interlocked.Increment(ref _id);
        }

        public void Start()
        {
            if (_tokenSource == null)
                lock (_syncRoot)
                    if (_tokenSource == null)
                    {
                        _tokenSource = new CancellationTokenSource();
                        _task = Task.Factory.StartNew(() => RunnerLoop(),
                            _tokenSource.Token,
                            TaskCreationOptions.LongRunning,
                            TaskScheduler.Current);
                    }
        }

        public int ID { get; private set; }
        public JobType JobType { get; private set; }
        public DateTime LastExecution { get; private set; }

        public void Stop()
        {
            if (_tokenSource != null)
                lock (_syncRoot)
                    if (_tokenSource != null)
                    {
                        _tokenSource.Cancel();
                    }
        }

        public void WaitForExit(int millisecondsTimeout = -1)
        {
            if (_task != null)
                _task.Wait(millisecondsTimeout);
        }

        private void RunnerLoop()
        {
            while (true)
            {
                if (_tokenSource.IsCancellationRequested)
                    break;

                var context = _scheduler.GetNextJobContext(JobType);
                if (context != null)
                {
                    var jobName = context.Job.GetType().FullName;
                    if (!string.IsNullOrEmpty(context.Name))
                        jobName += " (" + context.Name + ")";
                    try
                    {
                        var utcStart = DateTime.UtcNow;
                        LastExecution = utcStart;

                        context.UtcStart = utcStart;

                        context.OnExecuting(this, context);
                        context.ExecutionTimes++;
                        Logger.Info("Job {0} executing on runner #{1}", jobName, ID);
                        context.Job.Execute(context);
                        context.UtcLastEnd = DateTime.UtcNow;
                        context.UtcLastStart = utcStart;
                        context.UtcStart = null;
                        Logger.Info("Job {0} executed on runner #{1}, elapsed {2}", jobName, ID, context.UtcLastEnd - context.UtcLastStart);

                        context.OnExecuted(this, context);

                        context.LastException = null;
                    }
                    catch (Exception ex)
                    {
                        context.LastException = ex;
                        Logger.Error("Job {0} exception on runner #{1}" + Environment.NewLine + "{2}", jobName, ID, ex);
                    }
                    context.Scheduler.JobExecuted(context);
                }
                else
                    Task.Delay(IntervalOfTask).Wait();
            }

            _tokenSource = null;
            Logger.Info("Exit schedule runner #{0} loop", ID);
        }
    }
}