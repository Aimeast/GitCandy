using GitCandy.Controllers;
using System.Web.Mvc;

namespace GitCandy.Filters
{
    public class ReadRepositoryAttribute : SmartAuthorizeAttribute
    {
        private bool requireWrite;

        public ReadRepositoryAttribute(bool requireWrite = false)
        {
            this.requireWrite = requireWrite;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            var controller = filterContext.Controller as CandyControllerBase;
            if (controller != null && controller.Token != null && controller.Token.IsSystemAdministrator)
                return;

            var repoController = controller as RepositoryController;
            if (repoController != null)
            {
                var username = controller.Token == null ? null : controller.Token.Username;
                var field = controller.ValueProvider.GetValue("name");
                var canRead = field != null && (requireWrite
                    ? repoController.RepositoryService.CanWriteRepository(field.AttemptedValue, username)
                    : repoController.RepositoryService.CanReadRepository(field.AttemptedValue, username));
                if (canRead)
                    return;
            }

            HandleUnauthorizedRequest(filterContext);
        }
    }
}