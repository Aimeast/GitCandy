using GitCandy.Configuration;
using GitCandy.Controllers;
using System.Web.Mvc;

namespace GitCandy.Filters
{
    public class AllowRepositoryCreationAttribute : SmartAuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            var controller = filterContext.Controller as CandyControllerBase;
            if (controller != null && controller.Token != null
                && (UserConfiguration.Current.AllowRepositoryCreation
                    || controller.Token.IsSystemAdministrator))
                return;

            HandleUnauthorizedRequest(filterContext);
        }
    }
}