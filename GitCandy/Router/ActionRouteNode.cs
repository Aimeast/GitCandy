using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace GitCandy.Router
{
    public abstract class ActionRouteNode : RouteNode
    {
        protected string _controllerName;

        public string ControllerName => _controllerName;
    }

    public class ActionRouteNode<T> : ActionRouteNode
        where T : Controller
    {
        private readonly string _optional;
        private readonly ICollection<string> _actions;

        public ActionRouteNode(ICollection<string> actions, string optional = null)
        {
            _optional = string.IsNullOrEmpty(optional) ? null : optional;
            _actions = actions;
            var controllerName = typeof(T).Name;
            _controllerName = controllerName.Substring(0, controllerName.Length - 10);
        }


        public override int SkipSegment => 1;

        public override string FieldName => _optional == null ? null : "*" + _optional;

        public override RouteNode Add(RouteNode route)
        {
            throw new NotSupportedException();
        }

        public override IEnumerable<(string Key, string Value)> GetData(RouteMatchingStatus status)
        {
            var action = status.Segment[0];
            if (action == "")
                action = "Index";

            if (!_actions.Contains(_controllerName + "." + action))
            {
                return null;
            }

            var a = ("action", action);
            var c = ("controller", _controllerName);

            return status.Segment.Length == 1 || _optional == null
                ? new[] { a, c }
                : new[] { a, c, (_optional, string.Join('/', status.Segment.Slice(SkipSegment).ToArray())) };
        }
    }
}
