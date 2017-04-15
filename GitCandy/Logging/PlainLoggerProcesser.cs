using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace GitCandy.Logging
{
    // Microsoft.Extensions.Logging.Console.Internal.ConsoleLoggerProcessor.cs
    // Copyright (c) .NET Foundation. All rights reserved.
    // Modified by Aimeast.
    // Licensed under the Apache License, Version 2.0.

    public class PlainLoggerProcesser : IDisposable
    {
        private const int _maxQueuedMessages = 1024;

        private readonly BlockingCollection<string> _messageQueue = new BlockingCollection<string>(_maxQueuedMessages);
        private readonly Task _outputTask;
        private readonly IFileProvider _root;
        private readonly Func<LogFileExpiration> _formatter;

        private DelayCloseWriter _writter = null;
        private DateTime _lastExpire = DateTime.MinValue;
        private bool _canceled = false;

        public PlainLoggerProcesser(IFileProvider root, Func<LogFileExpiration> formatter = null)
        {
            _root = root;
            _formatter = formatter ?? DefaultLogfile;
            _outputTask = Task.Factory.StartNew(ProcessLogQueue, TaskCreationOptions.LongRunning);
        }

        public void EnqueueMessage(string message)
        {
            _messageQueue.Add(message);
        }

        private void ProcessLogQueue()
        {
            var spin = new SpinWait();
            foreach (var message in _messageQueue.GetConsumingEnumerable())
            {
                if (_writter == null || !_writter.CanWrite || DateTime.Now > _lastExpire)
                {
                    _writter?.Dispose();

                    var exp = _formatter();
                    _lastExpire = exp.Expiration;
                    var filename = exp.Filename;

                    _writter = new DelayCloseWriter(_root.GetFileInfo(filename).PhysicalPath);
                }

                if (_canceled)
                    break;

                _writter.Write(message);

                spin.SpinOnce();
            }
        }

        private LogFileExpiration DefaultLogfile()
        {
            var now = DateTime.Now;
            return new LogFileExpiration(now.ToString("yyyyMMdd") + ".log", now.Date.AddDays(1.0));
        }

        public void Dispose()
        {
            _messageQueue.CompleteAdding();

            try
            {
                _outputTask.Wait(1500);
            }
            catch (TaskCanceledException) { }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException) { }

            _canceled = true;
            Thread.Sleep(1);

            _writter.Dispose();
        }
    }
}
