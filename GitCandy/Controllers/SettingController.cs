using GitCandy.App_GlobalResources;
using GitCandy.Configuration;
using GitCandy.Filters;
using GitCandy.Git;
using GitCandy.Log;
using GitCandy.Models;
using GitCandy.Ssh;
using System;
using System.Composition;
using System.Web;
using System.Web.Mvc;

namespace GitCandy.Controllers
{
    [Administrator]
    [Export(typeof(SettingController))]
    public class SettingController : CandyControllerBase
    {
        public ActionResult Edit()
        {
            var config = UserConfiguration.Current;
            var model = new SettingModel
            {
                IsPublicServer = config.IsPublicServer,
                ForceSsl = config.ForceSsl,
                SslPort = config.SslPort,
                SshPort = config.SshPort,
                EnableSsh = config.EnableSsh,
                LocalSkipCustomError = config.LocalSkipCustomError,
                AllowRegisterUser = config.AllowRegisterUser,
                AllowRepositoryCreation = config.AllowRepositoryCreation,
                RepositoryPath = config.RepositoryPath,
                CachePath = config.CachePath,
                GitCorePath = config.GitCorePath,
                NumberOfCommitsPerPage = config.NumberOfCommitsPerPage,
                NumberOfItemsPerList = config.NumberOfItemsPerList,
                NumberOfRepositoryContributors = config.NumberOfRepositoryContributors,
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(SettingModel model)
        {
            var needRestart = false;
            var needRestartSshServer = false;

            if (ModelState.IsValid)
            {
                var config = UserConfiguration.Current;

                needRestart = (config.CachePath != model.CachePath);
                needRestartSshServer = config.SshPort != model.SshPort || config.EnableSsh != model.EnableSsh;

                config.IsPublicServer = model.IsPublicServer;
                config.ForceSsl = model.ForceSsl;
                config.SslPort = model.SslPort;
                config.SshPort = model.SshPort;
                config.EnableSsh = model.EnableSsh;
                config.LocalSkipCustomError = model.LocalSkipCustomError;
                config.AllowRegisterUser = model.AllowRegisterUser;
                config.AllowRepositoryCreation = model.AllowRepositoryCreation;
                config.RepositoryPath = model.RepositoryPath;
                config.CachePath = model.CachePath;
                config.GitCorePath = model.GitCorePath;
                config.NumberOfCommitsPerPage = model.NumberOfCommitsPerPage;
                config.NumberOfItemsPerList = model.NumberOfItemsPerList;
                config.NumberOfRepositoryContributors = model.NumberOfRepositoryContributors;
                config.Save();
                ModelState.Clear();
            }

            Logger.Info("Settings updated by {0}#{1}", Token.Username, Token.UserID);

            if (needRestart)
            {
                SshServerConfig.StopSshServer();
                HttpRuntime.UnloadAppDomain();
            }
            else if (needRestartSshServer)
            {
                SshServerConfig.RestartSshServer();
            }

            return View(model);
        }

        public ActionResult Restart(string conform)
        {
            if (string.Equals(conform, "yes", StringComparison.OrdinalIgnoreCase))
            {
                HttpRuntime.UnloadAppDomain();
                return RedirectToStartPage();
            }
            return View();
        }

        public ActionResult ReGenSsh(string conform)
        {
            if (string.Equals(conform, "yes", StringComparison.OrdinalIgnoreCase))
            {
                UserConfiguration.Current.HostKeys.Clear();
                foreach (var type in KeyUtils.SupportedAlgorithms)
                {
                    UserConfiguration.Current.HostKeys.Add(new HostKey { KeyType = type, KeyXml = KeyUtils.GeneratePrivateKey(type) });
                }
                UserConfiguration.Current.Save();

                SshServerConfig.RestartSshServer();

                return RedirectToAction("Edit");
            }
            return View();
        }
    }
}
