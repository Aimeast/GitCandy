using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace GitCandy.Schedules
{
    public class SingleJob : IJob
    {
        private Task _task;

        public JobTypes JobType { get; }

        public SingleJob(Action action, JobTypes jobType)
        {
            Contract.Requires(action != null);

            _task = new Task(action);
            JobType = jobType;
        }

        public SingleJob(Task task, JobTypes jobType)
        {
            Contract.Requires(task != null);

            _task = task;
            JobType = jobType;
        }

        public void Execute(JobContext jobContext)
        {
            _task.Start();
            _task.Wait();
        }

        public DateTime GetNextExecutionTime(JobContext jobContext)
        {
            return DateTime.MinValue;
        }
    }
}
