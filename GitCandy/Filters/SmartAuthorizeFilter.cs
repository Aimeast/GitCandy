using GitCandy.Security;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;

namespace GitCandy.Filters
{
    public class SmartAuthorizeFilter : IResourceFilter
    {
        public virtual void OnResourceExecuted(ResourceExecutedContext context)
        {
        }

        public virtual void OnResourceExecuting(ResourceExecutingContext context)
        {
        }

        protected virtual void HandleUnauthorizedRequest(ResourceExecutingContext context)
        {
            var token = context.HttpContext.Features.Get<Token>();
            if (token == null)
            {
                var helper = new UrlHelper(context);

                var retUrl = context.HttpContext.Request.GetEncodedPathAndQuery();
                var retObj = (string.IsNullOrEmpty(retUrl) || retUrl == "/")
                    ? null
                    : new { ReturnUrl = retUrl };

                context.Result = new RedirectResult(helper.Action("Login", "Home", retObj));
            }
            else if (token.IsSystemAdministrator)
            {
                context.Result = new NotFoundResult();
            }
            else
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
