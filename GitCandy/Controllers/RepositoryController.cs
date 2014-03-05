﻿using GitCandy.App_GlobalResources;
using GitCandy.Base;
using GitCandy.Configuration;
using GitCandy.Data;
using GitCandy.Filters;
using GitCandy.Git;
using GitCandy.Log;
using GitCandy.Models;
using System;
using System.Composition;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GitCandy.Controllers
{
    [Export(typeof(RepositoryController))]
    public class RepositoryController : CandyControllerBase
    {
        [Import]
        public RepositoryService RepositoryService { get; set; }

        public ActionResult Index()
        {
            var username = Token == null ? null : Token.Username;
            var model = RepositoryService.GetRepositories(username, Token != null && Token.IsSystemAdministrator);
            model.CanCreateRepository = Token != null
                && (UserConfiguration.Current.AllowRepositoryCreation
                    || Token.IsSystemAdministrator);
            return View(model);
        }

        [AllowRepositoryCreation]
        public ActionResult Create()
        {
            var model = new RepositoryModel
            {
                IsPrivate = false,
                AllowAnonymousRead = true,
                AllowAnonymousWrite = false,
            };
            return View(model);
        }

        [HttpPost]
        [AllowRepositoryCreation]
        public ActionResult Create(RepositoryModel model)
        {
            if (ModelState.IsValid)
            {
                bool badName;
                var repo = RepositoryService.Create(model, Token.UserID, out badName);
                if (repo != null)
                {
                    var success = GitService.CreateRepository(model.Name);
                    if (!success)
                    {
                        RepositoryService.Delete(model.Name);
                        repo = null;
                    }
                }
                if (repo != null)
                {
                    return RedirectToAction("Detail", "Repository", new { name = repo.Name });
                }
                if (badName)
                    ModelState.AddModelError("Name", SR.Repository_AlreadyExists);
            }

            return View(model);
        }

        [ReadRepository]
        public ActionResult Detail(string name)
        {
            var model = RepositoryService.GetRepositoryModel(name, true);
            if (model == null)
                throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
            using (var git = new GitService(GitService.GetDirectoryInfo(name).FullName))
            {
                model.DefaultBranch = git.GetHeadBranch();
            }
            return View(model);
        }

        [RepositoryOwnerOrSystemAdministrator]
        public ActionResult Edit(string name)
        {
            var model = RepositoryService.GetRepositoryModel(name);
            if (model == null)
                throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
            using (var git = new GitService(GitService.GetDirectoryInfo(name).FullName))
            {
                model.DefaultBranch = git.GetHeadBranch();
                model.LocalBranches = git.GetLocalBranches();
            }
            return View(model);
        }

        [HttpPost]
        [RepositoryOwnerOrSystemAdministrator]
        public ActionResult Edit(string name, RepositoryModel model)
        {
            if (string.IsNullOrEmpty(name))
                throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);

            if (ModelState.IsValid)
            {
                if (!RepositoryService.Update(model))
                    throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
                using (var git = new GitService(GitService.GetDirectoryInfo(name).FullName))
                {
                    git.SetHeadBranch(model.DefaultBranch);
                }
                return RedirectToAction("Detail", new { name });
            }

            return View(model);
        }

        [RepositoryOwnerOrSystemAdministrator]
        public ActionResult Coop(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);

            var model = RepositoryService.GetRepositoryCollaborationModel(name);
            return View(model);
        }

        [HttpPost]
        [RepositoryOwnerOrSystemAdministrator]
        public JsonResult ChooseUser(string name, string user, string act, string value)
        {
            string message = null;

            if (act == "add")
            {
                var role = RepositoryService.RepositoryAddUser(name, user);
                if (role != null)
                    return Json(new { role.AllowRead, role.AllowWrite, role.IsOwner });
            }
            else if (act == "del")
            {
                if (!Token.IsSystemAdministrator
                     && string.Equals(user, Token.Username, StringComparison.OrdinalIgnoreCase))
                    message = SR.Account_CantRemoveSelf;
                else if (RepositoryService.RepositoryRemoveUser(name, user))
                    return Json("success");
            }
            else if (act == "read" || act == "write" || act == "owner")
            {
                var val = string.Equals(bool.TrueString, value, StringComparison.OrdinalIgnoreCase);
                if (!Token.IsSystemAdministrator
                     && (act == "owner" && !val)
                     && string.Equals(user, Token.Username, StringComparison.OrdinalIgnoreCase))
                    message = SR.Account_CantRemoveSelf;
                else if (RepositoryService.RepositoryUserSetValue(name, user, act, val))
                    return Json("success");
            }

            Response.StatusCode = 400;
            return Json(message ?? SR.Shared_SomethingWrong);
        }

        [HttpPost]
        [RepositoryOwnerOrSystemAdministrator]
        public JsonResult ChooseTeam(string name, string team, string act, string value)
        {
            if (act == "add")
            {
                var role = RepositoryService.RepositoryAddTeam(name, team);
                if (role != null)
                    return Json(new { role.AllowRead, role.AllowWrite });
            }
            else if (act == "del")
            {
                if (RepositoryService.RepositoryRemoveTeam(name, team))
                    return Json("success");
            }
            else if (act == "read" || act == "write" || act == "owner")
            {
                var val = string.Equals(bool.TrueString, value, StringComparison.OrdinalIgnoreCase);
                if (RepositoryService.RepositoryTeamSetValue(name, team, act, val))
                    return Json("success");
            }

            Response.StatusCode = 400;
            return Json(SR.Shared_SomethingWrong);
        }

        [RepositoryOwnerOrSystemAdministrator]
        public ActionResult Delete(string name, string conform)
        {
            if (string.Equals(conform, "yes", StringComparison.OrdinalIgnoreCase))
            {
                RepositoryService.Delete(name);
                GitService.DeleteRepository(name);
                Logger.Info("Repository {0} deleted by {1}#{2}", name, Token.Username, Token.UserID);
                return RedirectToAction("Index");
            }
            return View((object)name);
        }

        [ReadRepository]
        public ActionResult Tree(string name, string path)
        {
            using (var git = new GitService(GitService.GetDirectoryInfo(name).FullName))
            {
                var model = git.GetTree(path);
                if (model == null)
                    throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
                if (model.Entries == null && model.ReferenceName != "HEAD")
                    return RedirectToAction("Tree", new { path = model.ReferenceName });

                model.GitUrl = GetGitUrl(name);
                model.RepositoryName = name;
                if (model.IsRoot)
                {
                    var m = RepositoryService.GetRepositoryModel(name);
                    model.Description = m.Description;
                }
                return View(model);
            }
        }

        [ReadRepository]
        public ActionResult Blob(string name, string path)
        {
            using (var git = new GitService(GitService.GetDirectoryInfo(name).FullName))
            {
                var model = git.GetBlob(path);
                if (model == null)
                    throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
                model.RepositoryName = name;
                return View(model);
            }
        }

        [ReadRepository]
        public ActionResult Blame(string name, string path)
        {
            using (var git = new GitService(GitService.GetDirectoryInfo(name).FullName))
            {
                var model = git.GetBlame(path);
                if (model == null)
                    throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
                model.RepositoryName = name;
                return View(model);
            }
        }

        [ReadRepository]
        public ActionResult Raw(string name, string path)
        {
            using (var git = new GitService(GitService.GetDirectoryInfo(name).FullName))
            {
                var model = git.GetBlob(path);
                if (model == null)
                    throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);

                return model.BlobType == BlobType.Binary
                    ? new RawResult(model.RawData, FileHelper.BinaryMimeType, model.Name)
                    : new RawResult(model.RawData);
            }
        }

        [ReadRepository]
        public ActionResult Commit(string name, string path)
        {
            using (var git = new GitService(GitService.GetDirectoryInfo(name).FullName))
            {
                var model = git.GetCommit(path);
                if (model == null)
                    throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
                model.RepositoryName = name;
                return View(model);
            }
        }

        [ReadRepository]
        public ActionResult Commits(string name, string path, int? page)
        {
            using (var git = new GitService(GitService.GetDirectoryInfo(name).FullName))
            {
                var model = git.GetCommits(path, page ?? 1, UserConfiguration.Current.NumberOfCommitsPerPage);
                if (model == null)
                    throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);

                ViewBag.Pager = Pager.Items(model.ItemCount)
                    .PerPage(UserConfiguration.Current.NumberOfCommitsPerPage)
                    .Move(model.CurrentPage)
                    .Segment(5)
                    .Center();

                model.RepositoryName = name;
                return View(model);
            }
        }

        [ReadRepository]
        public ActionResult Archive(string name, string path, string eol = null)
        {
            using (var git = new GitService(GitService.GetDirectoryInfo(name).FullName))
            {
                string newline = null;
                switch (eol)
                {
                    case "LF":
                        newline = "\n";
                        break;
                    case "CR":
                        newline = "\r";
                        break;
                    case "CRLF":
                        newline = "\r\n";
                        break;
                    default:
                        eol = null;
                        break;
                }

                string referenceName;
                var cacheFile = git.GetArchiveFilename(path, newline, out referenceName);
                if (cacheFile == null)
                    throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);

                var filename = name + "-" + referenceName;
                if (eol != null)
                    filename += "-" + eol;
                return File(cacheFile, "application/zip", filename + ".zip");
            }
        }

        [ReadRepository]
        public ActionResult Tags(string name)
        {
            using (var git = new GitService(GitService.GetDirectoryInfo(name).FullName))
            {
                var model = git.GetTags();
                if (model == null)
                    throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
                model.RepositoryName = name;
                return View(model);
            }
        }

        [ReadRepository]
        public ActionResult Branches(string name)
        {
            using (var git = new GitService(GitService.GetDirectoryInfo(name).FullName))
            {
                var model = git.GetBranches();
                if (model == null)
                    throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
                model.RepositoryName = name;
                return View(model);
            }
        }

        [ReadRepository]
        public ActionResult Contributors(string name, string path)
        {
            using (var git = new GitService(GitService.GetDirectoryInfo(name).FullName))
            {
                var model = git.GetContributors(path);
                if (model == null)
                    throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
                return View(model);
            }
        }

        private string GetGitUrl(string name)
        {
            var url = Request.Url;
            string path = VirtualPathUtility.ToAbsolute("~/git/" + name + ".git");
            UriBuilder ub = new UriBuilder(url.Scheme, url.Host, url.Port, path);
            return ub.Uri.ToString();
        }
    }
}
