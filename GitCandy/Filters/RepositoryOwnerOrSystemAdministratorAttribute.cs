using GitCandy.Controllers;
using System.Web.Mvc;

namespace GitCandy.Filters
{
    public class RepositoryOwnerOrSystemAdministratorAttribute : SmartAuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            var controller = filterContext.Controller as CandyControllerBase;
            if (controller != null && controller.Token != null)
            {
                if (controller.Token.IsSystemAdministrator)
                    return;

                var repoController = controller as RepositoryController;
                if (repoController != null)
                {
                    var field = controller.ValueProvider.GetValue("name");
                    var isAdmin = field != null && repoController.RepositoryService.IsRepositoryAdministrator(field.AttemptedValue, controller.Token.Username);
                    if (isAdmin)
                        return;
                }
            }

            HandleUnauthorizedRequest(filterContext);
        }
    }
}