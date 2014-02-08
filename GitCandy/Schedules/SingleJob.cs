using System;
using System.Composition;
using System.Diagnostics.Contracts;

namespace GitCandy.Schedules
{
    public class SingleJob : IJob
    {
        private Action _act;
        private double _secondDue;

        [ImportingConstructor]
        public SingleJob()
            : this(() => { }, 1.0)
        { }

        public SingleJob(Action action, double secondDue = 4.0)
        {
            Contract.Requires(action != null);

            _act = action;
            _secondDue = secondDue;
        }

        public void Execute(JobContext jobContext)
        {
            _act();
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