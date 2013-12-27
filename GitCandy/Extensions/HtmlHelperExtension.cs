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
        public static RouteValueDictionary OverRoute(this HtmlHelper helper, object routeValues = null)
        {
            var old = helper.ViewContext.RouteData.Values;

            if (routeValues == null)
                return old;

            var values = new RouteValueDictionary(routeValues);
            var over = new Dictionary<string, object>(old, StringComparer.OrdinalIgnoreCase);
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
    }
}