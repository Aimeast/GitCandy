﻿using System.Web.Optimization;

namespace GitCandy
{
    public static class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/bundles/css").Include(
                        //"~/Content/bootstrap.flatly.css",
                        "~/Content/bootstrap.cosmo.css",
                        "~/Content/bootstrapSwitch.css",
                        "~/Content/highlight.css",
                        "~/Content/site.css"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/bootstrapSwitch.js",
                        "~/Scripts/bootstrap3-typeahead.js",
                        "~/Scripts/highlight.pack.js",
                        "~/Scripts/marked.js",
                        "~/Scripts/ZeroClipboard.js",
                        "~/Scripts/common.js"
                        ));
        }
    }
}