using GitCandy.App_GlobalResources;
using GitCandy.Base;
using GitCandy.Configuration;
using GitCandy.Filters;
using GitCandy.Models;
using System;
using System.Composition;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GitCandy.Controllers
{
    [Export(typeof(TeamController))]
    public class TeamController : CandyControllerBase
    {
        [Administrator]
        public ActionResult Index(string query, int? page)
        {
            var model = MembershipService.GetTeamList(query, page ?? 1, UserConfiguration.Current.NumberOfItemsPerList);

            ViewBag.Pager = Pager.Items(model.ItemCount)
                .PerPage(UserConfiguration.Current.NumberOfItemsPerList)
                .Move(model.CurrentPage)
                .Segment(5)
                .Center();

            return View(model);
        }

        [Administrator]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Administrator]
        public ActionResult Create(TeamModel model)
        {
            if (ModelState.IsValid)
            {
                bool badName;
                var team = MembershipService.CreateTeam(model.Name, model.Description, Token.UserID, out badName);
                if (team != null)
                {
                    return RedirectToAction("Detail", "Team", new { team.Name });
                }
                if (badName)
                    ModelState.AddModelError("Name", SR.Team_AlreadyExists);
            }

            return View(model);
        }

        public ActionResult Detail(string name)
        {
            var model = MembershipService.GetTeamModel(name, true, Token == null ? null : Token.Username);
            if (model == null)
                throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
            return View(model);
        }

        [TeamOrSystemAdministrator]
        public ActionResult Edit(string name)
        {
            var model = MembershipService.GetTeamModel(name);
            if (model == null)
                throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
            ModelState.Clear();

            return View(model);
        }

        [HttpPost]
        [TeamOrSystemAdministrator]
        public ActionResult Edit(string name, TeamModel model)
        {
            if (ModelState.IsValid)
                if (!MembershipService.UpdateTeam(model))
                    throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);

            return View(model);
        }

        [TeamOrSystemAdministrator]
        public ActionResult Users(string name)
        {
            var model = MembershipService.GetTeamModel(name, true);
            return View(model);
        }

        [HttpPost]
        [TeamOrSystemAdministrator]
        public JsonResult ChooseUser(string name, string user, string act)
        {
            string message = null;
            if (act == "add")
            {
                if (MembershipService.TeamAddUser(name, user))
                    return Json("success");
            }
            else if (act == "del")
            {
                if (!Token.IsSystemAdministrator
                    && string.Equals(user, Token.Username, StringComparison.OrdinalIgnoreCase))
                    message = SR.Account_CantRemoveSelf;
                else if (MembershipService.TeamRemoveUser(name, user))
                    return Json("success");
            }
            else if (act == "admin" || act == "member")
            {
                var isAdmin = act == "admin";
                if (!Token.IsSystemAdministrator
                    && !isAdmin && string.Equals(user, Token.Username, StringComparison.OrdinalIgnoreCase))
                    message = SR.Account_CantRemoveSelf;
                else if (MembershipService.TeamUserSetAdministrator(name, user, isAdmin))
                    return Json("success");
            }

            Response.StatusCode = 400;
            return Json(message ?? SR.Shared_SomethingWrong);
        }

        [Administrator]
        public ActionResult Delete(string name, string confirm)
        {
            if (string.Equals(confirm, "yes", StringComparison.OrdinalIgnoreCase))
            {
                MembershipService.DeleteTeam(name);
                return RedirectToAction("Index");
            }
            return View((object)name);
        }

        [HttpPost]
        public JsonResult Search(string query)
        {
            var result = MembershipService.SearchTeam(query);
            return Json(result);
        }
    }
}
