using System;
using System.Composition;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace GitCandy.Schedules
{
    public class SingleJob : IJob
    {
        private Task _task;
        private JobType _jobType;

        [ImportingConstructor]
        public SingleJob()
            : this(() => { }, JobType.RealTime)
        { }

        public SingleJob(Action action, JobType jobType = JobType.LongRunning)
        {
            Contract.Requires(action != null);

            _task = new Task(action);
            _jobType = jobType;
        }

        public SingleJob(Task task, JobType jobType = JobType.LongRunning)
        {
            Contract.Requires(task != null);

            _task = task;
            _jobType = jobType;
        }

        public void Execute(JobContext jobContext)
        {
            _task.Start();
            _task.Wait();
        }

        public TimeSpan GetNextInterval(JobContext jobContext)
        {
            return TimeSpan.MaxValue;
        }

        public JobType JobType
        {
            get { return _jobType; }
        }
    }
}