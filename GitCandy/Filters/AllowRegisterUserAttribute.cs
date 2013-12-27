using GitCandy.Configuration;
using GitCandy.Controllers;
using System.Web.Mvc;

namespace GitCandy.Filters
{
    public class AllowRegisterUserAttribute : SmartAuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            var controller = filterContext.Controller as CandyControllerBase;
            var currentUser = controller == null ? null : controller.Token;
            if (currentUser != null && currentUser.IsSystemAdministrator)
                return;

            if (currentUser == null && UserConfiguration.Current.AllowRegisterUser)
                return;

            HandleUnauthorizedRequest(filterContext);
        }
    }
}