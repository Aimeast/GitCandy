using GitCandy.Configuration;
using GitCandy.Controllers;
using System;
using System.Text;
using System.Web.Mvc;

namespace GitCandy.Filters
{
    public class SmartGitAttribute : SmartAuthorizeAttribute
    {
        private const string AuthKey = "GitCandyGitAuthorize";

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            var controller = filterContext.Controller as GitController;
            if (controller == null)
                return;

            // git.exe not accept cookies as well as no session available
            var username = controller.Session[AuthKey] as string;
            if (username == null)
            {
                var token = controller.Token;
                if (token != null)
                {
                    username = token.Username;
                }
            }
            if (username == null)
            {
                var auth = controller.HttpContext.Request.Headers["Authorization"];

                if (!String.IsNullOrEmpty(auth))
                {
                    var bytes = Convert.FromBase64String(auth.Substring(6));
                    var certificate = Encoding.ASCII.GetString(bytes);
                    var index = certificate.IndexOf(':');
                    var password = certificate.Substring(index + 1);
                    username = certificate.Substring(0, index);

                    var user = controller.MembershipService.Login(username, password);
                    username = user != null ? user.Name : null;
                }
            }

            controller.Session[AuthKey] = username;

            if (username == null && !UserConfiguration.Current.IsPublicServer)
            {
                HandleUnauthorizedRequest(filterContext);
                return;
            }

            var right = false;

            var projectField = controller.ValueProvider.GetValue("project");
            var serviceField = controller.ValueProvider.GetValue("service");

            var project = projectField == null ? null : projectField.AttemptedValue;
            var service = serviceField == null ? null : serviceField.AttemptedValue;

            if (string.IsNullOrEmpty(service)) // redirect to git browser
            {
                right = true;
            }
            else if (string.Equals(service, "git-receive-pack", StringComparison.OrdinalIgnoreCase)) // git push
            {
                right = controller.RepositoryService.CanWriteRepository(project, username);
            }
            else if (string.Equals(service, "git-upload-pack", StringComparison.OrdinalIgnoreCase)) // git fetch
            {
                right = controller.RepositoryService.CanReadRepository(project, username);
            }

            if (!right)
                HandleUnauthorizedRequest(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var controller = filterContext.Controller as CandyControllerBase;
            if (controller == null || controller.Token == null)
            {
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.AddHeader("WWW-Authenticate", "Basic realm=\"Git Candy\"");
                filterContext.Result = new HttpUnauthorizedResult();
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}