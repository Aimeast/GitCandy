using System;
using System.Collections.Generic;

namespace GitCandy.Router
{
    public class RouteTree : RouteNode
    {
        public override int SkipSegment => 0;

        public override string FieldName => throw new NotSupportedException();

        public override IEnumerable<(string Key, string Value)> GetData(RouteMatchingStatus status)
        {
            return EmptyRouteData;
        }
    }
}
