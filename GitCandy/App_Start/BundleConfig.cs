using System.Web.Optimization;

namespace GitCandy
{
    public static class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/bundles/css").Include(
                        "~/Content/bootstrap.css",
                        "~/Content/bootstrap-switch.css",
                        "~/Content/highlight.css",
                        "~/Content/site.css"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/bootstrap-switch.js",
                        "~/Scripts/bootstrap3-typeahead.js",
                        "~/Scripts/highlight.pack.js",
                        "~/Scripts/marked.js",
                        "~/Scripts/common.js"
                        ));
        }
    }
}