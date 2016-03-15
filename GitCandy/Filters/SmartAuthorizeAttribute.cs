using GitCandy.Controllers;
using System;
using System.Web;
using System.Web.Mvc;

namespace GitCandy.Filters
{
    public class SmartAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var controller = filterContext.Controller as CandyControllerBase;
            if (controller == null || controller.Token == null)
            {
                var helper = new UrlHelper(filterContext.RequestContext);

                var retUrl = filterContext.HttpContext.Request.Url.PathAndQuery;
                var retObj = (string.IsNullOrEmpty(retUrl) || retUrl == "/")
                    ? null
                    : new { ReturnUrl = filterContext.HttpContext.Request.Url.PathAndQuery };

                filterContext.Result = new RedirectResult(helper.Action("Login", "Account", retObj));
            }
            else if (controller.Token.IsSystemAdministrator)
            {
                throw new HttpException(404, "Project not found.");
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}