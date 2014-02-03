using GitCandy.Log;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GitCandy.Schedules
{
    internal sealed class Runner
    {
        private readonly object _syncRoot = new object();
        private readonly Scheduler _scheduler;

        private CancellationTokenSource _tokenSource;

        public Runner(Scheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public void Start()
        {
            if (_tokenSource == null)
                lock (_syncRoot)
                    if (_tokenSource == null)
                    {
                        _tokenSource = new CancellationTokenSource();
                        Task.Factory.StartNew(() => RunnerLoop(),
                            _tokenSource.Token,
                            TaskCreationOptions.LongRunning,
                            TaskScheduler.Current);
                    }
        }

        public void Stop()
        {
            if (_tokenSource != null)
                lock (_syncRoot)
                    if (_tokenSource != null)
                    {
                        _tokenSource.Cancel();
                        _tokenSource = null;
                    }
        }

        private void RunnerLoop()
        {
            while (true)
            {
                if (_tokenSource.IsCancellationRequested)
                    break;

                var context = _scheduler.GetNextJobContext();
                if (context != null)
                {
                    var jobName = context.Job.GetType().FullName;
                    try
                    {
                        var utcNow = DateTime.UtcNow;
                        context.UtcStart = utcNow;

                        context.OnExecuting(this, context);
                        Logger.Info("Job " + jobName + " on executing");
                        context.Job.Execute(context);
                        Logger.Info("Job " + jobName + " on executed");
                        context.ExecutionTimes++;
                        context.UtcLastEnd = DateTime.UtcNow;
                        context.UtcLastStart = utcNow;
                        context.UtcStart = null;

                        context.Scheduler.JobExecuted(context);
                        context.OnExecuted(this, context);

                        context.LastException = null;
                    }
                    catch (Exception ex)
                    {
                        context.LastException = ex;
                        Logger.Error("Job " + jobName + " exception" + Environment.NewLine + ex.ToString());
                    }
                }

                if (_tokenSource.IsCancellationRequested)
                    break;

                Task.Delay(1000).Wait();
            }
        }
    }
}