using GitCandy.Configuration;
using GitCandy.Controllers;
using System.Web.Mvc;

namespace GitCandy.Filters
{
    public class PublicServerAttribute : SmartAuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (UserConfiguration.Current.IsPublicServer)
                return;

            bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
                || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true);

            if (skipAuthorization)
                return;

            base.OnAuthorization(filterContext);

            var controller = filterContext.Controller as CandyControllerBase;
            if (controller != null && controller.Token != null)
                return;
            
            HandleUnauthorizedRequest(filterContext);
        }
    }
}