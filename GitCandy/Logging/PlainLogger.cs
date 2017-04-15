using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Text;

namespace GitCandy.Logging
{
    // Microsoft.Extensions.Logging.Console.ConsoleLogger.cs
    // Copyright (c) .NET Foundation. All rights reserved.
    // Modified by Aimeast.
    // Licensed under the Apache License, Version 2.0.

    public class PlainLogger : ILogger
    {
        private readonly string _name;
        private readonly bool _includeScopes;
        private readonly PlainLoggerProcesser _messageQueue;

        [ThreadStatic]
        private static StringBuilder _logBuilder;

        public PlainLogger(string name, PlainLoggerProcesser messageQueue, bool includeScopes)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _messageQueue = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));
            _includeScopes = includeScopes;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return PlainLogScope.Push(_name, state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message) && exception == null)
                return;

            var logBuilder = _logBuilder ?? new StringBuilder();
            _logBuilder = null;

            logBuilder.Append("# ");
            logBuilder.Append(DateTimeOffset.Now.ToString(DateTimeFormatInfo.InvariantInfo));
            logBuilder.Append(", ");

            if (!string.IsNullOrEmpty(message))
            {
                logBuilder.Append(logLevel);
                logBuilder.Append(", ");
                logBuilder.Append(_name);
                logBuilder.Append(", [");
                logBuilder.Append(eventId);
                logBuilder.Append("], ");
                logBuilder.AppendLine(message);

                if (_includeScopes)
                {
                    var current = PlainLogScope.Current;

                    if (current != null)
                    {
                        logBuilder.Append(" @scope");
                        var length = logBuilder.Length;

                        do
                        {
                            logBuilder.Insert(length, $" => {current}");
                            current = current.Parent;
                        } while (current != null);

                        logBuilder.AppendLine();
                    }
                }
            }

            if (exception != null)
            {
                logBuilder.Append(" @");
                logBuilder.AppendLine(exception.ToString());
            }

            _messageQueue.EnqueueMessage(logBuilder.ToString());

            logBuilder.Clear();
            if (logBuilder.Capacity > 2048)
            {
                logBuilder.Capacity = 2048;
            }
            _logBuilder = logBuilder;
        }
    }
}
