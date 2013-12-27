using GitCandy.Filters;
using System.Web.Mvc;

namespace GitCandy
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new PublicServerAttribute());
            filters.Add(new CustomErrorAttribute());
        }
    }
}