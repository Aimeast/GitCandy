using GitCandy.Configuration;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace GitCandy.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class CustomErrorAttribute : FilterAttribute, IExceptionFilter, IActionFilter
    {
        public virtual void OnException(ExceptionContext filterContext)
        {
            if (UserConfiguration.Current.LocalSkipCustomError && filterContext.HttpContext.Request.IsLocal)
                return;

            var statusCode = new HttpException(null, filterContext.Exception).GetHttpCode();

            filterContext.Result = new HttpStatusCodeResult(statusCode, HttpWorkerRequest.GetStatusDescription(statusCode));
            filterContext.ExceptionHandled = true;

            var response = filterContext.HttpContext.Response;
            response.Clear();
            response.StatusCode = statusCode;
            response.TrySkipIisCustomErrors = true;

            var path = filterContext.HttpContext.Server.MapPath("~/CustomErrors/");
            var filename = Path.Combine(path, statusCode + ".html");
            if (File.Exists(filename))
            {
                response.WriteFile(filename);
            }
            else
            {
                filename = Path.Combine(path, "000.html");
                if (File.Exists(filename))
                {
                    var content = File.ReadAllText(filename);
                    response.Write(string.Format(content, statusCode));
                }
            }
        }

        public virtual void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}