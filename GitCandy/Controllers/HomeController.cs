using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GitCandy.Controllers
{
    public class HomeController : Controller
    {
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

            return new RedirectResult(string.IsNullOrEmpty(referer) ? "~/" : referer);
        }
    }
}
