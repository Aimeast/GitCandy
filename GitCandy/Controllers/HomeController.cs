using GitCandy.Data;
using GitCandy.Filters;
using GitCandy.Models;
using GitCandy.Resources;
using GitCandy.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;

namespace GitCandy.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataService _dataService;
        private readonly IStringLocalizer<SR> _sharedLocalizer;

        public HomeController(DataService dataService,
            IStringLocalizer<SR> sharedLocalizer)
        {
            _dataService = dataService;
            _sharedLocalizer = sharedLocalizer;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Lang(string lang)
        {
            Response.Cookies.Append("Lang", lang, new CookieOptions { Expires = DateTimeOffset.MaxValue });

            var referer = Request.Headers["Referer"].ToString();

            return Redirect(string.IsNullOrEmpty(referer) ? "~/" : referer);
        }

        [AllowAnonymous]
        [ServiceFilter(typeof(AllowRegisterUserFilter))]
        public IActionResult Join()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ServiceFilter(typeof(AllowRegisterUserFilter))]
        public IActionResult Join(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _dataService.UserManager.CreateUser(
                    model.Name,
                    model.Nickname,
                    model.Password,
                    model.Email,
                    model.Description,
                    out var badName,
                    out var badEmail);
                if (user != null)
                {
                    var auth = _dataService.UserManager.CreateAuthorization(user.ID,
                        Token.AuthorizationExpires,
                        HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString());
                    var token = new Token(auth.AuthCode, user.ID, user.Name, user.Nickname, user.IsSystemAdministrator);
                    HttpContext.Features.Set(token);

                    return RedirectToAction("index", "user", user.Name);
                }
                if (badName)
                {
                    ModelState.AddModelError("Name", _sharedLocalizer["User_UsernameAlreadyExists"]);
                }
                if (badEmail)
                {
                    ModelState.AddModelError("Email", _sharedLocalizer["User_EmailAlreadyExists"]);
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (HttpContext.Features.Get<Token>() != null)
            {
                return Redirect("~/");
            }

            var model = new LoginModel { ReturnUrl = Request.Headers["Referer"].ToString() };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(LoginModel model)
        {
            if (HttpContext.Features.Get<Token>() != null)
            {
                return Forbid();
            }

            var user = _dataService.UserManager.Login(model.Name, model.Password);
            if (user != null)
            {
                var auth = _dataService.UserManager.CreateAuthorization(user.ID,
                    Token.AuthorizationExpires,
                    HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString());
                var token = new Token(auth.AuthCode, user.ID, user.Name, user.Nickname, user.IsSystemAdministrator);
                HttpContext.Features.Set(token);

                return Redirect(model.ReturnUrl);
            }

            ModelState.AddModelError("", _sharedLocalizer["User_LoginFailed"]);
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Logout()
        {
            var token = HttpContext.Features.Get<Token>();
            if (token != null)
            {
                _dataService.UserManager.SetAuthorizationAsInvalid(token.AuthCode);
                HttpContext.Features.Set<Token>(null);
            }

            var referer = Request.Headers["Referer"].ToString();
            return Redirect(string.IsNullOrEmpty(referer) ? "~/" : referer);
        }
    }
}
