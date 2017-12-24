using GitCandy.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace GitCandy.Router
{
    public static class RouterExtensions
    {
        public static void AddGitCandyRouter(this IServiceCollection services)
        {
            services.AddSingleton<GitCandyRouter>();
        }

        public static void UseGitCandyRouter(this IApplicationBuilder app)
        {
            var middlewarePipelineBuilder = app.ApplicationServices.GetRequiredService<MiddlewareFilterBuilder>();
            middlewarePipelineBuilder.ApplicationBuilder = app.New();

            var router = app.ApplicationServices.GetRequiredService<GitCandyRouter>();
            var actions = router.DiscoveredActions;

            var root = new RouteTree()
                .Add(new ActionRouteNode<HomeController>(actions));

            router.BuildRoute(root);

            app.UseRouter(router);
        }
    }
}
