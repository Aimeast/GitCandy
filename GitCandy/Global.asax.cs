using GitCandy.Base;
using GitCandy.Configuration;
using GitCandy.Git.Cache;
using GitCandy.Log;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace GitCandy
{
    public class GitCandyApplication : System.Web.HttpApplication
    {
        public bool HidingRequestResponse { get; private set; }

        protected void Application_Start()
        {
            HidingRequestResponse = HttpRuntime.UsingIntegratedPipeline;

            Logger.SetLogPath();
            Logger.Info(AppInfomation.GetAppStartingInfo());

            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            MefConfig.RegisterMef();
            ScheduleConfig.RegisterScheduler();

            GitCacheAccessor.Initialize();

            Logger.Info(AppInfomation.GetAppStartedInfo());

            HidingRequestResponse = false;
        }

        protected void Application_End()
        {
            ScheduleConfig.StopAndWait();
            Logger.Info(AppInfomation.GetAppEndInfo());
        }

        protected void Application_Error()
        {
            var context = this.Context;
            var server = context.Server;
            var ex = server.GetLastError();
            var statusCode = new HttpException(null, ex).GetHttpCode();

            var sb = new StringBuilder();
            if (HidingRequestResponse)
                sb.AppendLine(statusCode + ", Unknow, First request on integrated mode");
            else
                sb.AppendLine(statusCode + ", " + context.Request.HttpMethod + ", " + context.Request.Url.ToString());
            if (statusCode == 500)
            {
                sb.AppendLine(ex.ToString());
                Logger.Error(sb.ToString());
            }
            else
            {
                Logger.Warning(sb.ToString());
            }

            if (!HidingRequestResponse && UserConfiguration.Current.LocalSkipCustomError && context.Request.IsLocal)
                return;

            if (!HidingRequestResponse)
            {
                var response = context.Response;
                response.Clear();
                response.StatusCode = statusCode;
                response.TrySkipIisCustomErrors = true;
                response.ContentType = @"text/html; charset=utf-8";

                var path = server.MapPath("~/CustomErrors/");
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

            server.ClearError();
        }

        protected void Application_AcquireRequestState()
        {
            if (HttpContext.Current.Session != null)
            {
                var culture = Session["Culture"] as CultureInfo;
                if (culture == null)
                {
                    string langName = "en-us";

                    if (Request.Cookies["Lang"] != null)
                    {
                        langName = Request.Cookies["Lang"].Value;
                    }
                    else if (HttpContext.Current.Request.UserLanguages != null && HttpContext.Current.Request.UserLanguages.Length != 0)
                    {
                        langName = HttpContext.Current.Request.UserLanguages[0].Split(';')[0];
                    }
                    try
                    {
                        culture = CultureInfo.CreateSpecificCulture(langName);
                    }
                    catch
                    {
                        culture = CultureInfo.CreateSpecificCulture("en-us");
                    }

                    var cookie = new HttpCookie("Lang", culture.Name)
                    {
                        Expires = DateTime.Now.AddYears(2),
                    };
                    Response.Cookies.Set(cookie);

                    Session["Culture"] = culture;
                }
                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        protected void Application_BeginRequest()
        {
            Profiler.Start();
        }
    }
}