using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace GitCandy.Extensions
{
    public static class HtmlHelperExtension
    {
        public static RouteValueDictionary OverRoute(this HtmlHelper helper, object routeValues = null, bool withQuery = false)
        {
            var old = helper.ViewContext.RouteData.Values;

            if (routeValues == null)
                return old;

            var over = new Dictionary<string, object>(old, StringComparer.OrdinalIgnoreCase);
            if (withQuery)
            {
                var qs = helper.ViewContext.HttpContext.Request.QueryString;
                foreach (string key in qs)
                    over[key] = qs[key];
            }
            var values = new RouteValueDictionary(routeValues);
            foreach (var pair in values)
                over[pair.Key] = pair.Value;

            return new RouteValueDictionary(over);
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, RouteValueDictionary routeValues, object htmlAttributes)
        {
            return LinkExtensions.ActionLink(htmlHelper, linkText, actionName, routeValues, htmlAttributes.CastToDictionary());
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, RouteValueDictionary routeValues, object htmlAttributes)
        {
            return LinkExtensions.ActionLink(htmlHelper, linkText, actionName, controllerName, routeValues, htmlAttributes.CastToDictionary());
        }

        public static MvcHtmlString CultureActionLink(this HtmlHelper htmlHelper, string langName)
        {
            var culture = CultureInfo.CreateSpecificCulture(langName);
            var displayName = culture.Name.StartsWith("en")
                ? culture.NativeName
                : culture.EnglishName + " - " + culture.NativeName;

            return LinkExtensions.ActionLink(htmlHelper, displayName, "Language", "Home", new { Lang = culture.Name }, null);
        }

        public static dynamic GetRootViewBag(this HtmlHelper html)
        {
            var controller = html.ViewContext.Controller;
            while (controller.ControllerContext.IsChildAction)
            {
                controller = controller.ControllerContext.ParentActionViewContext.Controller;
            }
            return controller.ViewBag;
        }
    }
}