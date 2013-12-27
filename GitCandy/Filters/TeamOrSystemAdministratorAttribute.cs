using GitCandy.Controllers;
using System.Web.Mvc;

namespace GitCandy.Filters
{
    public class TeamOrSystemAdministratorAttribute : SmartAuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            var controller = filterContext.Controller as CandyControllerBase;
            if (controller != null && controller.Token != null)
            {
                if (controller.Token.IsSystemAdministrator)
                    return;

                var field = controller.ValueProvider.GetValue("name");
                var isAdmin = field != null && controller.MembershipService.IsTeamAdministrator(field.AttemptedValue, controller.Token.Username);
                if (isAdmin)
                    return;
            }

            HandleUnauthorizedRequest(filterContext);
        }
    }
}