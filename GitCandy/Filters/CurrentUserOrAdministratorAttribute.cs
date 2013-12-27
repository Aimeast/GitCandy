using GitCandy.Controllers;
using System.Web.Mvc;

namespace GitCandy.Filters
{
    public class CurrentUserOrAdministratorAttribute : SmartAuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            var controller = filterContext.Controller as CandyControllerBase;
            if (controller != null && controller.Token != null)
            {
                if (controller.Token.IsSystemAdministrator)
                    return;

                var field = filterContext.Controller.ValueProvider.GetValue("name");
                if (field == null || string.IsNullOrEmpty(field.AttemptedValue) || controller.Token.Username == field.AttemptedValue)
                    return;
            }

            HandleUnauthorizedRequest(filterContext);
        }
    }
}