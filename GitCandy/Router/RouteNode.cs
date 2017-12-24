using System;
using System.Collections.Generic;

namespace GitCandy.Router
{
    public abstract class RouteNode
    {
        private readonly List<RouteNode> _children = new List<RouteNode>();
        private readonly List<RouteNode> _parents = new List<RouteNode>();

        public static (string, string)[] EmptyRouteData = new(string, string)[0];

        public IEnumerable<RouteNode> Children => _children;
        public IEnumerable<RouteNode> Parents => _parents;

        public virtual RouteNode Add(RouteNode route)
        {
            if (route is RouteTree)
                throw new NotSupportedException();

            _children.Add(route);
            route._parents.Add(this);
            return this;
        }

        public abstract IEnumerable<(string Key, string Value)> GetData(RouteMatchingStatus status);
        public abstract int SkipSegment { get; }
        public abstract string FieldName { get; }
    }
}
