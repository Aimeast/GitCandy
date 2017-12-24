using System;
using System.Collections.Generic;

namespace GitCandy.Router
{
    public class ConstantRouteNode : RouteNode
    {
        private readonly string _constant;

        public ConstantRouteNode(string constant)
        {
            _constant = constant;
        }

        public override int SkipSegment => 1;

        public override string FieldName => "@" + _constant;

        public override IEnumerable<(string Key, string Value)> GetData(RouteMatchingStatus status)
        {
            return string.Equals(_constant, status.Segment[0], StringComparison.OrdinalIgnoreCase)
                ? EmptyRouteData
                : null;
        }
    }
}
