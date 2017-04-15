using System;

namespace GitCandy.Logging
{
    public class LogFileExpiration
    {
        public LogFileExpiration(string filename, DateTime expiration)
        {
            Filename = filename ?? throw new ArgumentNullException(nameof(filename));
            Expiration = expiration;
        }

        public string Filename { get; }

        public DateTime Expiration { get; }
    }
}
