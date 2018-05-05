using GitCandy.Schedules;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GitCandy.Tests
{
    public class SchedulerTests
    {
        [Fact]
        public void TestScheduledQueue()
        {
            var logger = new Mock<ILoggerFactory>();
            var runner = new Runner(new CancellationToken(), logger.Object, JobTypes.ScheduledQuickly, 1);

            Assert.Null(runner.GetNextContext());

            var now = DateTime.Now;
            var job1context = CreateMockJobContext(JobTypes.ScheduledQuickly, now.AddSeconds(1), "j1");
            var job2context = CreateMockJobContext(JobTypes.ScheduledQuickly, now.AddSeconds(3), "j2");
            var job3context = CreateMockJobContext(JobTypes.ScheduledQuickly, now.AddSeconds(2), "j3");
            var job4context = CreateMockJobContext(JobTypes.ScheduledQuickly, now.AddSeconds(2), "j4");
            runner.Push(job1context);
            runner.Push(job2context);

            Assert.Null(runner.GetNextContext());
            Task.Delay(1000).Wait();
            Assert.Same(job1context, runner.GetNextContext());

            runner.Push(job3context);
            runner.Push(job4context);

            Assert.Null(runner.GetNextContext());
            Task.Delay(1000).Wait();
            Assert.Same(job3context, runner.GetNextContext());
            Assert.Same(job4context, runner.GetNextContext());
            Assert.Null(runner.GetNextContext());
            Task.Delay(1000).Wait();
            Assert.Same(job2context, runner.GetNextContext());
            Assert.Null(runner.GetNextContext());
            Assert.Null(runner.GetNextContext());
        }

        [Fact]
        public void TestOnceQueue()
        {
            var logger = new Mock<ILoggerFactory>();
            var runner = new Runner(new CancellationToken(), logger.Object, JobTypes.OnceQuickly, 1);

            Assert.Null(runner.GetNextContext());

            var now = DateTime.Now;
            var job1context = CreateMockJobContext(JobTypes.OnceQuickly, now.AddSeconds(1), "j1");
            var job2context = CreateMockJobContext(JobTypes.OnceQuickly, now.AddSeconds(3), "j2");
            var job3context = CreateMockJobContext(JobTypes.OnceQuickly, now.AddSeconds(2), "j3");
            var job4context = CreateMockJobContext(JobTypes.OnceQuickly, now.AddSeconds(2), "j4");
            runner.Push(job1context);
            runner.Push(job2context);
            runner.Push(job3context);
            runner.Push(job4context);

            Assert.Same(job1context, runner.GetNextContext());
            Assert.Same(job2context, runner.GetNextContext());
            Assert.Same(job3context, runner.GetNextContext());
            Assert.Same(job4context, runner.GetNextContext());
        }

        [Fact]
        public void TestJobException()
        {
            var logger = new Mock<ILoggerFactory>();
            logger.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
            var runner = new Runner(new CancellationToken(), logger.Object, JobTypes.OnceQuickly, 2);

            var job1 = new Mock<IJob>();
            var context1 = new JobContext(job1.Object);
            job1.Setup(x => x.JobType).Returns(JobTypes.OnceQuickly);
            job1.Setup(x => x.Execute(context1)).Throws<Exception>();

            var job2 = new Mock<IJob>();
            var context2 = new JobContext(job2.Object);
            job2.Setup(x => x.JobType).Returns(JobTypes.OnceQuickly);

            runner.Start();
            runner.Push(context1);
            runner.Push(context2);

            Task.Delay(1000).Wait();

            Assert.NotNull(context1.LastException);
            Assert.Null(context2.LastException);
        }

        [Fact]
        public void TestQuicklyOrLongly()
        {
            var logger = new Mock<ILoggerFactory>();
            var runner1 = new Runner(new CancellationToken(), logger.Object, JobTypes.OnceLongly, 1);
            var runner2 = new Runner(new CancellationToken(), logger.Object, JobTypes.OnceQuickly, 1);
            var runner3 = new Runner(new CancellationToken(), logger.Object, JobTypes.ScheduledLongly, 1);
            var runner4 = new Runner(new CancellationToken(), logger.Object, JobTypes.ScheduledQuickly, 1);

            var now = DateTime.Now;
            var job1context = CreateMockJobContext(JobTypes.OnceLongly, now.AddSeconds(0.5), "j1");
            var job2context = CreateMockJobContext(JobTypes.OnceQuickly, now.AddSeconds(0.5), "j2");
            var job3context = CreateMockJobContext(JobTypes.ScheduledLongly, now.AddSeconds(0.5), "j3");
            var job4context = CreateMockJobContext(JobTypes.ScheduledQuickly, now.AddSeconds(0.5), "j4");

            runner1.Push(job1context);
            runner2.Push(job2context);
            runner3.Push(job3context);
            runner4.Push(job4context);

            Task.Delay(1000).Wait();

            Assert.Same(job1context, runner1.GetNextContext());
            Assert.Same(job2context, runner2.GetNextContext());
            Assert.Same(job3context, runner3.GetNextContext());
            Assert.Same(job4context, runner4.GetNextContext());
        }

        [Fact]
        public void TestScheduler()
        {
            var lifetime = new Mock<IApplicationLifetime>();
            var logger = new Mock<ILoggerFactory>();
            logger.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
            var scheduler = new Scheduler(lifetime.Object, logger.Object);
            scheduler.Start();

            var count = 0;
            var datetime = DateTime.Now.AddSeconds(0.5);

            scheduler.AddJob(CreateMockJob(JobTypes.OnceLongly, datetime, x => Interlocked.Increment(ref count)));
            scheduler.AddJob(CreateMockJob(JobTypes.OnceQuickly, datetime, x => Interlocked.Increment(ref count)));
            scheduler.AddJob(CreateMockJob(JobTypes.ScheduledLongly, datetime, x => Interlocked.Increment(ref count)));
            scheduler.AddJob(CreateMockJob(JobTypes.ScheduledQuickly, datetime, x => Interlocked.Increment(ref count)));

            Task.Delay(1000).Wait();

            Assert.Equal(4, count);
        }

        [Fact]
        public void TestCancellation()
        {
            var logger = new Mock<ILoggerFactory>();
            var source = new CancellationTokenSource();
            var runner = new Runner(source.Token, logger.Object, JobTypes.ScheduledQuickly, 2);
            var job = CreateMockJobContext(JobTypes.ScheduledQuickly, DateTime.Now.AddSeconds(0.5), "j1");
            runner.Start();
            runner.Push(job);
            source.Cancel();
            Task.Delay(1000).Wait();

            Assert.Equal(default(DateTime), job.LastStarting);
        }

        private JobContext CreateMockJobContext(JobTypes jobType, DateTime datetime, string name)
        {
            var job = new Mock<IJob>();
            var context = new JobContext(job.Object) { Name = name };
            job.Setup(x => x.JobType).Returns(jobType);
            job.Setup(x => x.GetNextExecutionTime(context)).Returns(datetime);
            return context;
        }

        private IJob CreateMockJob(JobTypes jobType, DateTime datetime, Action<JobContext> act)
        {
            var job = new Mock<IJob>();
            var context = new JobContext(job.Object);
            job.Setup(x => x.JobType).Returns(JobTypes.OnceQuickly);
            job.Setup(x => x.GetNextExecutionTime(context)).Returns(datetime);
            job.Setup(x => x.Execute(It.IsAny<JobContext>())).Callback(act);
            return job.Object;
        }
    }
}
