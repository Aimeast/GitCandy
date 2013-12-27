using GitCandy.Controllers;
using System.Web.Mvc;

namespace GitCandy.Filters
{
    public class AdministratorAttribute : SmartAuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            var controller = filterContext.Controller as CandyControllerBase;
            if (controller == null || controller.Token == null || !controller.Token.IsSystemAdministrator)
                HandleUnauthorizedRequest(filterContext);
        }
    }
}