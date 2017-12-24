using GitCandy.Router;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace GitCandy.Tests
{
    public class RouterTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        private readonly GitCandyRouter _router;

        public RouterTests()
        {
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<TestRouterStartup>());
            _client = _server.CreateClient();
            _router = _server.Host.Services.GetService<GitCandyRouter>();
        }

        [Fact]
        public void CheckRouteNodeConstraint()
        {
            Assert.Throws<NotSupportedException>(() => new RouteTree().Add(new RouteTree()));
            Assert.Throws<NotSupportedException>(() => new DynamicRouteNode(null).Add(new ConstantRouteNode(null)));
            Assert.Throws<NotSupportedException>(() => new ActionRouteNode<Controller>(null, null).Add(new ConstantRouteNode(null)));
        }

        [Fact]
        public void RouteWithGetMethod()
        {
            Assert.Equal("Home.Index", GetContent(""));
            Assert.Equal("Home.Index", GetContent("/"));
            Assert.Equal("Home.Detail", GetContent("/detail"));
            Assert.Equal("Home.Detail", GetContent("/DETAIL/"));

            Assert.Equal("User.Index", GetContent("/Jack"));
            Assert.Equal("User.Detail+Jack", GetContent("/Jack/detail"));
            Assert.Equal("User.Detail+Lucy", GetContent("/Lucy/detail"));
            Assert.Equal("Team.Index", GetContent("/Moon/"));
            Assert.Equal("Team.Detail+Moon", GetContent("/Moon/detail"));
            Assert.Equal("Team.Detail+Dot", GetContent("/Dot/detail"));

            Assert.Equal("Repo.Index", GetContent("/Jack/Candy/"));
            Assert.Equal("Repo.Index", GetContent("/Moon/Candy/"));
            Assert.Equal("Repo.Detail+Jack,,Candy", GetContent("/Jack/Candy/detail"));
            Assert.Equal("Repo.Detail+,Moon,Candy", GetContent("/Moon/Candy/detail"));
            Assert.Equal("Repo.Detail+,Moon,Candy", GetContent("/Moon/Candy/detail/la"));

            Assert.Equal("Wiki.Index", GetContent("/Jack/Candy/wiki/"));
            Assert.Equal("Wiki.Index", GetContent("/Moon/Candy/WIKI/"));
            Assert.Equal("Wiki.Detail+Jack,,Candy,la/", GetContent("/Jack/Candy/wiki/detail/la"));
            Assert.Equal("Wiki.Detail+,Moon,Candy,a/b/", GetContent("/Moon/Candy/wiki/detail/a/b/"));

            Assert.Equal(HttpStatusCode.NotFound, _client.GetAsync("/Alen").Result.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, _client.GetAsync("/Jack/Candy/Foo").Result.StatusCode);
        }

        [Fact]
        public void RouteWithPostMethod()
        {
            var content = new FormUrlEncodedContent(new[] {
                KeyValuePair.Create("ID", ""),
                KeyValuePair.Create("post", "2"),
                KeyValuePair.Create("real", "TRUE"),
                KeyValuePair.Create("Content", "yes"),
            });
            var response = _client.PostAsync("/Jack/Candy/wiki/detail", content).Result;
            Assert.Equal("Wiki.Detail+,2,True,yes", response.Content.ReadAsStringAsync().Result);
        }

        public static IEnumerable<object[]> GenerateUrlTestData
        {
            get
            {
                yield return new object[] { "/",
                    new { Controller = "Home", Action = "Index" }, new { } };
                yield return new object[] { "/Join",
                    new { Controller = "Home", Action = "Join" }, new { controller = "Home", action = "Join"} };
                yield return new object[] { "/Jack",
                    new { Controller = "User", Action = "Index" }, new { Username = "Jack" } };
                yield return new object[] { "/Moon/Edit/a/b?app=foo&bar=dot",
                    new { Controller = "Team", Action = "Edit" }, new { teamname = "Moon", path = "a/b", app = "foo", bar="dot" } };
                yield return new object[] { "/Moon/Edit/a/b?bar=dot",
                    new { Controller = "Team", Action = "Edit", app = "foo" }, new { teamname = "Moon", path = "a/b", bar="dot" } };
                yield return new object[] { "/Lucy/Candy/wiki/Page/a/b",
                    new { Controller = "Wiki", Action = "Page" }, new { username = "Lucy", reponame="Candy", path = "a/b" } };
                yield return new object[] { "/",
                    new { Controller = "Wiki", Action = "Page" }, new { } }; // fail
            }
        }

        [Theory]
        [MemberData(nameof(GenerateUrlTestData))]
        public void GenerateUrl(string expected, object ambientValues, object values)
        {
            var context = new VirtualPathContext(null,
                new RouteValueDictionary(ambientValues),
                new RouteValueDictionary(values));

            Assert.Equal(expected, _router.GetVirtualPath(context).VirtualPath);
            Assert.Equal(expected, _router.GetVirtualPath(context).VirtualPath); // no effected
        }

        private string GetContent(string url)
        {
            return _client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
        }

        class TestRouterStartup
        {
            private (string Name, string Type)[] _names = new[] {
                ("Jack", "username"),
                ("Lucy", "username"),
                ("Moon", "teamname"),
                ("Dot", "teamname"),
            };

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddMvc()
                    .ConfigureApplicationPartManager(manager =>
                    {
                        var part = manager.ApplicationParts.FirstOrDefault(x => x.Name == "GitCandy");
                        manager.ApplicationParts.Remove(part);
                    });
                services.AddSingleton<GitCandyRouter>();
            }

            public void Configure(IApplicationBuilder app)
            {
                var router = app.ApplicationServices.GetRequiredService<GitCandyRouter>();
                var actions = router.DiscoveredActions;

                var repoRouteNode = new FieldRouteNode("reponame")
                    .Add(new ActionRouteNode<RepoController>(actions))
                    .Add(new ConstantRouteNode("wiki")
                        .Add(new ActionRouteNode<WikiController>(actions, "path"))
                    );

                var root = new RouteTree()
                    .Add(new ActionRouteNode<HomeController>(actions, "path"))
                    .Add(new DynamicRouteNode(s => _names.FirstOrDefault(x => x.Name == s).Type)
                        .Add(new ChoiceRouteNode("username")
                            .Add(new ActionRouteNode<UserController>(actions, "path"))
                            .Add(repoRouteNode)
                        )
                        .Add(new ChoiceRouteNode("teamname")
                            .Add(new ActionRouteNode<TeamController>(actions, "path"))
                            .Add(repoRouteNode)
                        )
                    );

                router.BuildRoute(root);

                app.UseRouter(router);
            }
        }
    }

    public class WikiModel
    {
        public int? ID { get; set; }
        public int Post { get; set; }
        public bool Real { get; set; }
        public string Content { get; set; }
    }

    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return Content("User.Index");
        }

        public IActionResult Detail(string username)
        {
            return Content("User.Detail+" + username);
        }
    }
    public class TeamController : Controller
    {
        public IActionResult Index()
        {
            return Content("Team.Index");
        }

        public IActionResult Detail(string teamname)
        {
            return Content("Team.Detail+" + teamname);
        }
    }
    public class RepoController : Controller
    {
        public IActionResult Index()
        {
            return Content("Repo.Index");
        }

        public IActionResult Detail(string username, string teamname, string reponame)
        {
            return Content("Repo.Detail+" + username + "," + teamname + "," + reponame);
        }
    }
    public class WikiController : Controller
    {
        public IActionResult Index()
        {
            return Content("Wiki.Index");
        }

        public IActionResult Detail(string username, string teamname, string reponame, string path)
        {
            return Content("Wiki.Detail+" + username + "," + teamname + "," + reponame + "," + path);
        }

        [HttpPost]
        public IActionResult Detail(WikiModel model)
        {
            return Content($"Wiki.Detail+{model.ID},{model.Post},{model.Real},{model.Content}");
        }
    }
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Content("Home.Index");
        }

        public IActionResult Detail()
        {
            return Content("Home.Detail");
        }
    }
}
