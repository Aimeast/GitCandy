using GitCandy.Base;
using GitCandy.Schedules;
using System.Composition.Convention;
using System.Web.Mvc;

namespace GitCandy
{
    public static class MefConfig
    {
        public static void RegisterMef()
        {
            var builder = RegisterExports();
            var resolver = DependencyResolver.Current;
            var newResolver = new MefDependencyResolver(builder, resolver);
            DependencyResolver.SetResolver(newResolver);
        }

        private static AttributedModelProvider RegisterExports()
        {
            var builder = new ConventionBuilder();
            //builder.ForType<learner>().Export<learner>();
            //builder.ForTypesMatching
            //    (x => x.GetProperty("SourceMaterial") != null).Export<exam>();

            builder.ForTypesDerivedFrom<IJob>().Export<IJob>();

            return builder;
        }
    }
}