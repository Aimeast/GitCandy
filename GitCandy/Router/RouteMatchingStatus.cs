using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System;
using System.Collections.Generic;

namespace GitCandy.Router
{
    public class RouteMatchingStatus
    {
        public RouteMatchingStatus(RouteNode node, string method, Span<string> segment, IQueryCollection query, IDictionary<string, string> routeData)
        {
            RouteNode = node;
            Method = method;
            Segment = segment;
            Query = query;
            RouteData = routeData;
        }

        public RouteNode RouteNode { get; private set; }
        public string Method { get; private set; }
        public Span<string> Segment { get; private set; }
        public IQueryCollection Query { get; private set; }
        public IDictionary<string, string> RouteData { get; private set; }

        public int SkipSegment { get; set; }
    }
}
