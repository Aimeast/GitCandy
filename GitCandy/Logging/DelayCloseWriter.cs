using System;
using System.IO;
using System.Threading.Tasks;

namespace GitCandy.Logging
{
    // This class is designed to non thread-safe purpose
    public class DelayCloseWriter : IDisposable
    {
        public const int MillisecondsOfDelay = 2000;
        public const int MillisecondsOfChecking = 200;

        private readonly string _filepath;

        private DateTime _next;
        private StreamWriter _writer = null;

        public DelayCloseWriter(string filepath)
        {
            _filepath = filepath;
            Task.Factory.StartNew(Observe, TaskCreationOptions.LongRunning);
        }

        public bool CanWrite { get; private set; } = true;

        public void Observe()
        {
            while (CanWrite)
            {
                if (_writer != null && DateTime.Now > _next)
                    CloseInternalWriter();

                Task.Delay(MillisecondsOfChecking).Wait();
            }
        }

        public void Write(string text)
        {
            _next = DateTime.Now.AddMilliseconds(MillisecondsOfDelay);

            EnsureWritter();

            _writer.Write(text);
            _writer.Flush();
        }

        private void EnsureWritter()
        {
            if (_writer == null)
            {
                _writer = new StreamWriter(new FileStream(_filepath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            }
        }

        private void CloseInternalWriter()
        {
            if (_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }
        }

        public void Dispose()
        {
            CanWrite = false;
            _writer?.Dispose();
        }
    }
}
