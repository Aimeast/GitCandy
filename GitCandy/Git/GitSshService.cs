using GitCandy.Configuration;
using GitCandy.Data;
using GitCandy.Ssh;
using GitCandy.Ssh.Services;
using System;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GitCandy.Git
{
    public class GitSshService
    {
        private readonly static Regex RegexParseCommand =
            new Regex(@"(?<cmd>git-receive-pack|git-upload-pack|git-upload-archive) \'/?git/(?<proj>.+)\.git\'", RegexOptions.Compiled);
        private readonly Session _session = null;
        private SessionChannel _channel = null;
        private Process _process = null;
        private MembershipService _membershipService = new MembershipService();
        private RepositoryService _repositoryService = new RepositoryService();

        public GitSshService(Session session)
        {
            _session = session;

            session.ServiceRegistered += ServiceRegistered;
        }

        private void ServiceRegistered(object sender, SshService e)
        {
            if (e is UserauthService)
            {
                var service = (UserauthService)e;
                service.Userauth += Userauth;
            }
            else if (e is ConnectionService)
            {
                var service = (ConnectionService)e;
                service.CommandOpened += CommandOpened;
            }
        }

        private void Userauth(object sender, UserauthArgs e)
        {
            e.Result = _membershipService.HasSshKey(e.Fingerprint);
        }

        private void CommandOpened(object sender, SessionRequestedArgs e)
        {
            var match = RegexParseCommand.Match(e.CommandText);
            var command = match.Groups["cmd"].Value;
            var project = match.Groups["proj"].Value;

            if (string.IsNullOrWhiteSpace(command) || string.IsNullOrWhiteSpace(project))
                throw new SshConnectionException("Unexpected command.", DisconnectReason.ByApplication);

            var requireWrite = command == "git-receive-pack";
            var fingerprint = e.AttachedUserauthArgs.Fingerprint;
            var key = Convert.ToBase64String(e.AttachedUserauthArgs.Key);
            var allow = requireWrite
                ? _repositoryService.CanWriteRepository(project, fingerprint, key)
                : _repositoryService.CanReadRepository(project, fingerprint, key);

            if (!allow)
                throw new SshConnectionException("Access denied.", DisconnectReason.ByApplication);

            e.Channel.DataReceived += DataReceived;
            e.Channel.EofReceived += EofReceived;
            e.Channel.CloseReceived += CloseReceived;
            _channel = e.Channel;

            StartProcess(command, project);
        }

        private void CloseReceived(object sender, EventArgs e)
        {
            EnsureClose();
        }

        private void EofReceived(object sender, EventArgs e)
        {
            _process.StandardInput.BaseStream.Close();
        }

        private void DataReceived(object sender, byte[] e)
        {
            _process.StandardInput.BaseStream.Write(e, 0, e.Length);
            _process.StandardInput.BaseStream.Flush();
        }

        private void StartProcess(string command, string project)
        {
            var args = Path.Combine(UserConfiguration.Current.RepositoryPath, project);

            var info = new ProcessStartInfo(Path.Combine(UserConfiguration.Current.GitCorePath, command + ".exe"), args)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            _process = Process.Start(info);

            Task.Run(() => MessageLoop());
        }

        private void MessageLoop()
        {
            var bytes = new byte[1024 * 64];
            while (true)
            {
                var len = _process.StandardOutput.BaseStream.Read(bytes, 0, bytes.Length);
                if (len <= 0)
                    break;

                var data = bytes.Length != len
                    ? bytes.Take(len).ToArray()
                    : bytes;
                _channel.SendData(data);
            }
            _channel.SendEof();
            _channel.SendClose((uint)_process.ExitCode);
            EnsureClose();
        }

        private void EnsureClose()
        {
            if (_channel.ClientClosed && _channel.ServerClosed)
                _session.Disconnect();
        }
    }
}
