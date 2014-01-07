using GitCandy.App_GlobalResources;
using GitCandy.Configuration;
using GitCandy.Filters;
using GitCandy.Git;
using GitCandy.Models;
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
                LocalSkipCustomError = config.LocalSkipCustomError,
                AllowRegisterUser = config.AllowRegisterUser,
                AllowRepositoryCreation = config.AllowRepositoryCreation,
                RepositoryPath = config.RepositoryPath,
                CachePath = config.CachePath,
                GitExePath = config.GitExePath,
                NumberOfCommitsPerPage = config.NumberOfCommitsPerPage,
                NumberOfItemsPerList = config.NumberOfItemsPerList,
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(SettingModel model)
        {
            var needRestart = false;

            if (ModelState.IsValid)
            {
                var verify = GitService.VerifyGit(model.GitExePath);
                if (!verify)
                {
                    ModelState.AddModelError("GitExePath", string.Format(SR.Validation_Filepath, "GitExePath", "git.exe"));
                    return View(model);
                }

                var config = UserConfiguration.Current;

                needRestart = (config.CachePath != model.CachePath);

                config.IsPublicServer = model.IsPublicServer;
                config.ForceSsl = model.ForceSsl;
                config.SslPort = model.SslPort;
                config.LocalSkipCustomError = model.LocalSkipCustomError;
                config.AllowRegisterUser = model.AllowRegisterUser;
                config.AllowRepositoryCreation = model.AllowRepositoryCreation;
                config.RepositoryPath = model.RepositoryPath;
                config.CachePath = model.CachePath;
                config.GitExePath = model.GitExePath;
                config.NumberOfCommitsPerPage = model.NumberOfCommitsPerPage;
                config.NumberOfItemsPerList = model.NumberOfItemsPerList;
                config.Save();
                ModelState.Clear();
            }

            if (needRestart)
            {
                HttpRuntime.UnloadAppDomain();
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
    }
}
