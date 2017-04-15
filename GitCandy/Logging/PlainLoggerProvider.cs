using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace GitCandy.Logging
{
    public class PlainLoggerProvider : ILoggerProvider
    {
        private readonly bool _includeScopes;

        private readonly PlainLoggerProcesser _messageQueue;
        private readonly ConcurrentDictionary<string, PlainLogger> _loggers = new ConcurrentDictionary<string, PlainLogger>();

        public PlainLoggerProvider(IFileProvider root, bool includeScopes = false)
        {
            _messageQueue = new PlainLoggerProcesser(root ?? throw new ArgumentNullException(nameof(root)));
            _includeScopes = includeScopes;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, _ => new PlainLogger(categoryName, _messageQueue, _includeScopes));
        }

        public void Dispose()
        {
            _messageQueue.Dispose();
        }
    }
}
