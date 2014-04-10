using System.Composition;
using System.Web;
using System.Web.Mvc;

namespace GitCandy.Controllers
{
    [Export(typeof(HomeController))]
    public class HomeController : CandyControllerBase
    {
        public ActionResult Index()
        {
            return RedirectToStartPage();
        }

        public ActionResult About()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Language(string lang)
        {
            var cookie = new HttpCookie("Lang", lang);
            Response.Cookies.Set(cookie);

            Session["Culture"] = null;

            if (Request.UrlReferrer == null)
                return RedirectToStartPage();
            return Redirect(Request.UrlReferrer.PathAndQuery);
        }
    }
}
