using System.Web.Optimization;

namespace GitCandy
{
    public static class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"
                        ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/site.css"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/bootstrapSwitch.js"
                        ));

            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                        "~/Content/bootstrap.css",
                        "~/Content/bootstrapSwitch.css"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/highlight").Include(
                        "~/Scripts/highlight.pack.js",
                        "~/Scripts/marked.js"
                        ));

            bundles.Add(new StyleBundle("~/Content/highlight").Include(
                        "~/Content/highlight.css"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/ZeroClipboard").Include(
                        "~/Scripts/ZeroClipboard.js"
                        ));
        }
    }
}