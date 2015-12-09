using System;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

namespace GitCandy.Log
{
    public class Logger
    {
        private static readonly object _staticSyncRoot = new object();
        private static readonly string _logPathFormat = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["LogPathFormat"]);
        private static Logger _instance;

        private readonly object _instanceSyncRoot = new object();
        private TextWriter _writer = null;
        private Timer _timer;
        private DateTime _utcLastWrite = DateTime.MinValue;

        private Logger(string logPath)
        {
            Contract.Requires(logPath != null);

            LogFilePath = logPath;
            _timer = new Timer(o => DisposeWriter(false), null, TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.1));
        }

        public string LogFilePath { get; private set; }

        public static void SetLogPath(string path = null)
        {
            lock (_staticSyncRoot)
            {
                if (_instance != null)
                {
                    _instance.DisposeWriter(true);
                    _instance._timer.Dispose();
                }

                if (string.IsNullOrEmpty(path))
                    path = string.Format(_logPathFormat, DateTime.Now.ToString("yyyyMMdd"));

                _instance = new Logger(path);
            }
        }

        private void DisposeWriter(bool force)
        {
            if (_writer != null)
                lock (_instanceSyncRoot)
                    if (_writer != null && (force || _utcLastWrite.AddSeconds(0.2) < DateTime.UtcNow))
                    {
                        _writer.Flush();
                        _writer.Dispose();
                        _writer = null;
                    }
        }

        private void InternalWrite(LogLevels level, string message)
        {
            lock (_instanceSyncRoot)
            {
                if (_writer == null)
                {
                    for (var times = 0; times < 5; times++)
                    {
                        try
                        {
                            _writer = new StreamWriter(new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8);
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(130);
                        }
                    }
                }

                if (_writer == null)
                    return;

                _writer.Write(">> " + DateTimeOffset.Now.ToString("MM/dd/yyyy HH:mm:ss.fff zzz") + " " + level + ", ");

                _writer.WriteLine((message ?? string.Empty).Trim());
                _writer.Flush();
                _utcLastWrite = DateTime.UtcNow;
            }
        }

        public static void Write(LogLevels level, string message)
        {
            _instance.InternalWrite(level, message);
        }

        public static void Write(LogLevels level, string format, params object[] args)
        {
            _instance.InternalWrite(level, string.Format(format, args));
        }

        public static void Info(string message)
        {
            _instance.InternalWrite(LogLevels.Info, message);
        }

        public static void Info(string format, params object[] args)
        {
            _instance.InternalWrite(LogLevels.Info, string.Format(format, args));
        }

        public static void Warning(string message)
        {
            _instance.InternalWrite(LogLevels.Warning, message);
        }

        public static void Warning(string format, params object[] args)
        {
            _instance.InternalWrite(LogLevels.Warning, string.Format(format, args));
        }

        public static void Error(string message)
        {
            _instance.InternalWrite(LogLevels.Error, message);
        }

        public static void Error(string format, params object[] args)
        {
            _instance.InternalWrite(LogLevels.Error, string.Format(format, args));
        }
    }
}