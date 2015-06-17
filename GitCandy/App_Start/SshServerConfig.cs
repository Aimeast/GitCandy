using GitCandy.Configuration;
using GitCandy.Git;
using GitCandy.Log;
using GitCandy.Ssh;
using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading.Tasks;

namespace GitCandy
{
    public static class SshServerConfig
    {
        private static SshServer _server = null;

        public static void StartSshServer()
        {
            if (!UserConfiguration.Current.EnableSsh || _server != null)
                return;

            _server = new SshServer(new StartingInfo(IPAddress.IPv6Any, UserConfiguration.Current.SshPort));
            _server.ConnectionAccepted += (s, e) => new GitSshService(e);
            _server.ExceptionRasied += (s, e) => Logger.Error(e.ToString());
            foreach (var key in UserConfiguration.Current.HostKeys)
            {
                _server.AddHostKey(key.KeyType, key.KeyXml);
            }
            for (var i = 1; i <= 10; i++)
            {
                try
                {
                    _server.Start();
                    Logger.Info("SSH server started.");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Info("Attempt to start SSH server failed in {0} times. {1}", i, ex);
                    Task.Delay(1000).Wait();
                }
            }
        }

        public static void StopSshServer()
        {
            if (_server == null)
                return;

            _server.Stop();
            _server = null;

            Logger.Info("SSH server stoped.");
        }

        public static void RestartSshServer()
        {
            StopSshServer();
            StartSshServer();
        }
    }
}
