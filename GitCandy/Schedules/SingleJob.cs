using System;
using System.Composition;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace GitCandy.Schedules
{
    public class SingleJob : IJob
    {
        private Task _task;
        private double _secondDue;

        [ImportingConstructor]
        public SingleJob()
            : this(() => { }, 1.0)
        { }

        public SingleJob(Action action, double secondDue = 4.0)
        {
            Contract.Requires(action != null);

            _task = new Task(action);
            _secondDue = secondDue;
        }

        public SingleJob(Task task, double secondDue = 4.0)
        {
            Contract.Requires(task != null);

            _task = task;
            _secondDue = secondDue;
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

        public TimeSpan Due
        {
            get { return TimeSpan.FromSeconds(_secondDue); }
        }
    }
}