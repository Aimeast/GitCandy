using GitCandy.Logging;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace GitCandy.Tests
{
    public class LoggingTests
    {
        [Fact]
        public void LoggerCreaterHasCache()
        {
            var fileProvider = GetFileProvider();
            var loggerProvider = new PlainLoggerProvider(fileProvider);
            var one = loggerProvider.CreateLogger("a");
            var two = loggerProvider.CreateLogger("a");
            var three = loggerProvider.CreateLogger("b");

            Assert.Same(one, two);
            Assert.NotSame(one, three);
        }

        [Fact]
        public void ChangeLogFileNamePattern()
        {
            var fileProvider = GetFileProvider();
            var fileList = new List<string>(4);
            using (var processer = new PlainLoggerProcesser(fileProvider, () =>
            {
                var now = DateTime.Now;
                var file = now.Ticks + ".log";
                fileList.Add(file);
                return new LogFileExpiration(file, now.AddSeconds(1.0));
            }))
            {
                processer.EnqueueMessage("1");
                Task.Delay(1100).Wait();
                processer.EnqueueMessage("2");
                Task.Delay(900).Wait();
                processer.EnqueueMessage("3");
            }
            Assert.Equal(2, fileList.Count);
            Assert.Equal("1", File.ReadAllText(fileProvider.GetFileInfo(fileList[0]).PhysicalPath));
            Assert.Equal("23", File.ReadAllText(fileProvider.GetFileInfo(fileList[1]).PhysicalPath));
        }

        [Fact]
        public void CreateDelayCloseWriterWithoutWriting()
        {
            var fileProvider = GetFileProvider();
            var filename = fileProvider.GetFileInfo(DateTime.Now.Ticks + ".log").PhysicalPath;

            var writter = new DelayCloseWriter(filename);

            Assert.True(writter.CanWrite);
            Assert.False(File.Exists(filename));

            writter.Dispose();

            Assert.False(writter.CanWrite);
        }

        [Fact]
        public void WriteAndReleaseLogFile()
        {
            var fileProvider = GetFileProvider();
            var filename = fileProvider.GetFileInfo(DateTime.Now.Ticks + ".log").PhysicalPath;

            var writter = new DelayCloseWriter(filename);
            writter.Write("Delay ");
            writter.Write("close");

            Assert.True(writter.CanWrite);
            Assert.True(File.Exists(filename));
            Assert.Equal("Delay close", ReadAllTextWithFileShare(filename));
            Assert.Throws<IOException>(() => File.Open(filename, FileMode.Open, FileAccess.Write).Dispose());

            Task.Delay(DelayCloseWriter.MillisecondsOfDelay + DelayCloseWriter.MillisecondsOfChecking).Wait();

            Assert.True(writter.CanWrite); // keep writing ability
            File.Open(filename, FileMode.Open, FileAccess.ReadWrite).Dispose();

            writter.Write(" testing");
            writter.Dispose();

            Assert.False(writter.CanWrite);
            Assert.Equal("Delay close testing", ReadAllTextWithFileShare(filename));
            File.Open(filename, FileMode.Open, FileAccess.ReadWrite).Dispose();
        }

        private IFileProvider GetFileProvider()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "testlog");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fileProvider = new PhysicalFileProvider(path);

            return fileProvider;
        }

        private string ReadAllTextWithFileShare(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }
    }
}
