using System;
using System.Web.Mvc;

namespace GitCandy.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class CustomErrorAttribute : FilterAttribute, IExceptionFilter, IActionFilter
    {
        public virtual void OnException(ExceptionContext filterContext)
        {
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