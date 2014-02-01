using GitCandy.App_GlobalResources;
using GitCandy.Base;
using GitCandy.Configuration;
using GitCandy.Filters;
using GitCandy.Log;
using GitCandy.Models;
using GitCandy.Security;
using System;
using System.Composition;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GitCandy.Controllers
{
    [Export(typeof(AccountController))]
    public class AccountController : CandyControllerBase
    {
        [Administrator]
        public ActionResult Index(string query, int? page)
        {
            var model = MembershipService.GetUserList(query, page ?? 1, UserConfiguration.Current.NumberOfItemsPerList);

            ViewBag.Pager = Pager.Items(model.ItemCount)
                .PerPage(UserConfiguration.Current.NumberOfItemsPerList)
                .Move(model.CurrentPage)
                .Segment(5)
                .Center();

            return View(model);
        }

        [AllowAnonymous]
        [AllowRegisterUser]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [AllowRegisterUser]
        public ActionResult Create(UserModel model)
        {
            if (ModelState.IsValid)
            {
                bool badName, badEmail;
                var user = MembershipService.CreateAccount(model.Name, model.Nickname, model.Password, model.Email, model.Description, out badName, out badEmail);
                if (user != null)
                {
                    if (Token != null)
                    {
                        return RedirectToAction("Detail", "Account", new { name = user.Name });
                    }
                    var auth = MembershipService.CreateAuthorization(user.ID, Token.AuthorizationExpires, Request.UserHostAddress);
                    Token = new Token(auth.AuthCode, user.ID, user.Name, user.Nickname, user.IsSystemAdministrator);
                    return RedirectToStartPage();
                }
                if (badName)
                    ModelState.AddModelError("Name", SR.Account_AccountAlreadyExists);
                if (badEmail)
                    ModelState.AddModelError("Email", SR.Account_EmailAlreadyExists);
            }

            return View(model);
        }

        public ActionResult Detail(string name)
        {
            if (string.IsNullOrEmpty(name) && Token != null)
                name = Token.Username;

            var model = MembershipService.GetUserModel(name, true, Token == null ? null : Token.Username);
            if (model == null)
                throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Logout(string returnUrl)
        {
            Token = null;
            return RedirectToStartPage(returnUrl);
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (Token != null)
                return RedirectToStartPage(returnUrl);

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            var user = MembershipService.Login(model.ID, model.Password);
            if (user != null)
            {
                var auth = MembershipService.CreateAuthorization(user.ID, Token.AuthorizationExpires, Request.UserHostAddress);
                Token = new Token(auth.AuthCode, user.ID, user.Name, user.Nickname, user.IsSystemAdministrator);

                return RedirectToStartPage(returnUrl);
            }

            ModelState.AddModelError("", SR.Account_LoginFailed);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [CurrentUserOrAdministrator]
        public ActionResult Change()
        {
            return View();
        }

        [HttpPost]
        [CurrentUserOrAdministrator]
        public ActionResult Change(ChangePasswordModel model, string name)
        {
            if (string.IsNullOrEmpty(name))
                name = Token.Username;

            var isAdmin = Token.IsSystemAdministrator
                && !string.Equals(name, Token.Username, StringComparison.OrdinalIgnoreCase);
            if (ModelState.IsValid)
            {
                var user = MembershipService.Login(isAdmin ? Token.Username : name, model.OldPassword);
                if (user != null)
                {
                    MembershipService.SetPassword(name, model.NewPassword);
                    if (!isAdmin)
                    {
                        var auth = MembershipService.CreateAuthorization(user.ID, Token.AuthorizationExpires, Request.UserHostAddress);
                        Token = new Token(auth.AuthCode, user.ID, user.Name, user.Nickname, user.IsSystemAdministrator);
                    }

                    return RedirectToAction("Detail", "Account", new { name });
                }
                ModelState.AddModelError("OldPassword", SR.Account_OldPasswordError);
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Forgot()
        {
            return View();
        }

        [CurrentUserOrAdministrator]
        public ActionResult Edit(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = Token.Username;

            var model = MembershipService.GetUserModel(name);
            if (model == null)
                throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
            ModelState.Clear();

            return View(model);
        }

        [HttpPost]
        [CurrentUserOrAdministrator]
        public ActionResult Edit(string name, UserModel model)
        {
            if (string.IsNullOrEmpty(name))
                name = Token.Username;

            var isAdmin = Token.IsSystemAdministrator
                && !string.Equals(name, Token.Username, StringComparison.OrdinalIgnoreCase);

            ModelState.Remove("ConformPassword");
            if (ModelState.IsValid)
            {
                var user = MembershipService.Login(isAdmin ? Token.Username : name, model.Password);
                if (user != null)
                {
                    model.IsSystemAdministrator = Token.IsSystemAdministrator && model.IsSystemAdministrator;
                    if (!Token.IsSystemAdministrator || isAdmin || model.IsSystemAdministrator)
                    {
                        if (!MembershipService.UpdateUser(model))
                            throw new HttpException((int)HttpStatusCode.NotFound, string.Empty);
                        if (!isAdmin)
                        {
                            Token = MembershipService.GetToken(Token.AuthCode);
                        }

                        return RedirectToAction("Detail", "Account", new { name });
                    }
                    ModelState.AddModelError("IsSystemAdministrator", SR.Account_CantRemoveSelf);
                }
                else
                    ModelState.AddModelError("Password", SR.Account_PasswordError);
            }
            return View(model);
        }

        [Administrator]
        public ActionResult Delete(string name, string conform)
        {
            if (string.Equals(Token.Username, name, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", SR.Account_CantRemoveSelf);
            }
            else if (string.Equals(conform, "yes", StringComparison.OrdinalIgnoreCase))
            {
                MembershipService.DeleteUser(name);
                Logger.Info("User {0} deleted by {1}#{2}", name, Token.Username, Token.UserID);
                return RedirectToAction("Index");
            }
            return View((object)name);
        }

        [HttpPost]
        public JsonResult Search(string query)
        {
            var result = MembershipService.SearchUsers(query);
            return Json(result);
        }
    }
}
