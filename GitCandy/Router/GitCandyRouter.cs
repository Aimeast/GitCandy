using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace GitCandy.Router
{
    public class GitCandyRouter : IRouter
    {
        private readonly IActionInvokerFactory _actionInvokerFactory;
        private readonly IActionSelector _actionSelector;
        private readonly ILogger _logger;
        private readonly HashSet<string> _actions;
        private readonly UrlEncoder _urlEncoder;

        private RouteNode _rootRoute;
        private string[][] _templates;

        public GitCandyRouter(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IActionInvokerFactory actionInvokerFactory,
            IActionSelector actionSelector,
            UrlEncoder urlEncoder,
            ILoggerFactory loggerFactory)
        {
            _actionInvokerFactory = actionInvokerFactory;
            _actionSelector = actionSelector;
            _urlEncoder = urlEncoder;
            _logger = loggerFactory.CreateLogger<GitCandyRouter>();

            var actions = actionDescriptorCollectionProvider
                .ActionDescriptors
                .Items
                .Select(x => x.RouteValues["controller"] + "." + x.RouteValues["action"]);
            _actions = new HashSet<string>(actions, StringComparer.OrdinalIgnoreCase);

            _rootRoute = new RouteTree();
        }

        public void BuildRoute(RouteNode rootRoute)
        {
            _rootRoute = rootRoute;

            // template[0] = controller name
            // template[1..] = key of routes.
            //   if start with '@', the field is constant.
            //   if start with '*', the field is optional.
            //   otherwish, is variable.
            var templates = new List<string[]>();
            var template = new string[32];
            foreach (var route in rootRoute.Children)
            {
                var stack = new Stack<(RouteNode, int)>();
                stack.Push((route, 1));
                while (stack.Count > 0)
                {
                    var (node, depth) = stack.Pop();
                    var recursive = node.FieldName != null;
                    if (node is ActionRouteNode action)
                    {
                        template[0] = action.ControllerName;
                        template[depth++] = "action";
                        if (action.FieldName != null) // as optional parameter
                        {
                            template[depth++] = action.FieldName;
                        }
                        templates.Add(template.Take(depth).ToArray());
                    }
                    else if (recursive)
                    {
                        template[depth] = node.FieldName;
                    }
                    foreach (var child in node.Children.Reverse())
                    {
                        stack.Push((child, depth + (recursive ? 1 : 0)));
                    }
                }
            }
            _templates = templates.ToArray();
        }

        public ICollection<string> DiscoveredActions => _actions;

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            var pathValues = GetPathValues(context, out var template);
            if (pathValues != null)
            {
                var path = string.Join('/', GetCombinedValues(pathValues, template));
                var querystring = string.Join('&', context.Values.Keys
                    .Except(template.Select(x => x.TrimStart('*')), StringComparer.OrdinalIgnoreCase)
                    .Select(x => _urlEncoder.Encode(x) + "=" + _urlEncoder.Encode(context.Values[x].ToString())));

                if (querystring != "")
                    path += "?" + querystring;

                return new VirtualPathData(this, "/" + path);
            }

            return new VirtualPathData(this, "/");
        }

        private string GetDictValue(RouteValueDictionary dict, string key)
        {
            dict.TryGetValue(key, out object value);
            return value as string;
        }

        private string[] GetPathValues(VirtualPathContext context, out string[] matchedTemplate)
        {
            matchedTemplate = null;
            var controller = GetDictValue(context.Values, "controller")
                ?? GetDictValue(context.AmbientValues, "controller");

            foreach (var template in _templates)
            {
                if (template[0] != controller)
                    continue;

                var matched = true;
                var values = new string[template.Length];
                for (var i = 1; i < template.Length; i++)
                {
                    var segment = template[i];
                    if (segment.StartsWith('@'))
                    {
                        values[i] = segment.Substring(1);
                    }
                    else
                    {
                        var isOptional = segment.StartsWith('*');
                        if (isOptional)
                        {
                            segment = segment.Substring(1);
                        }
                        var value = GetDictValue(context.Values, segment)
                            ?? GetDictValue(context.AmbientValues, segment);
                        if (!isOptional && string.IsNullOrEmpty(value))
                        {
                            matched = false;
                            break;
                        }

                        values[i] = value;
                    }
                }
                if (matched)
                {
                    matchedTemplate = template.ToArray();
                    matchedTemplate[0] = "controller";
                    return values;
                }
            }

            return null;
        }

        private IEnumerable<string> GetCombinedValues(string[] values, string[] template)
        {
            for (int i = 1; i < values.Length; i++)
            {
                if (template[i].StartsWith('*') && string.IsNullOrEmpty(values[i]))
                {
                    yield break;
                }
                if (string.Equals(template[i], "action", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(values[i], "index", StringComparison.OrdinalIgnoreCase))
                {
                    yield break;
                }
                yield return values[i];
            }
        }

        public Task RouteAsync(RouteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (MatchRouteData(context))
            {
                var actionDescriptor = SelectBestAction(context);
                if (actionDescriptor != null)
                {
                    context.Handler = _ =>
                    {
                        var actionContext = new ActionContext(context.HttpContext, context.RouteData, actionDescriptor);

                        var invoker = _actionInvokerFactory.CreateInvoker(actionContext);
                        if (invoker == null)
                            throw new InvalidOperationException();

                        return invoker.InvokeAsync();
                    };
                    return Task.CompletedTask;
                }
            }

            var request = context.HttpContext.Request;
            _logger.LogInformation($"Router not match: {request.Path}?{request.QueryString}");

            return Task.CompletedTask;
        }

        private bool MatchRouteData(RouteContext context)
        {
            var request = context.HttpContext.Request;
            var path = request.Path.Value;
            var status = new RouteMatchingStatus(_rootRoute,
                request.Method,
                new Span<string>((path.EndsWith('/') ? path : path + "/").Split('/')).Slice(1),
                request.Query,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            var stack = new Stack<NodeMatcher>(new[] { new NodeMatcher(status) });

            while (stack.Count > 0)
            {
                var matcher = stack.Pop();
                if (matcher.Match())
                {
                    if (matcher.Succeed)
                    {
                        foreach (var (key, value) in matcher.RouteData)
                        {
                            context.RouteData.Values.Add(key, value);
                        }

                        return true;
                    }
                    foreach (var child in matcher.GetChildren().Reverse())
                    {
                        stack.Push(child);
                    }
                }
            }

            return false;
        }

        private ActionDescriptor SelectBestAction(RouteContext context)
        {
            var candidates = _actionSelector.SelectCandidates(context);
            return candidates == null || candidates.Count == 0
                ? null
                : _actionSelector.SelectBestCandidate(context, candidates);
        }
    }
}
