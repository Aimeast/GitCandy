using System;
using System.Collections.Generic;

namespace GitCandy.Router
{
    public class DynamicRouteNode : RouteNode
    {
        private readonly Func<string, string> _selecter;

        public DynamicRouteNode(Func<string, string> selecter)
        {
            _selecter = selecter;
        }

        public override int SkipSegment => 1;

        public override string FieldName => null;

        public override RouteNode Add(RouteNode route)
        {
            if (route is ChoiceRouteNode)
                return base.Add(route);

            throw new NotSupportedException($"{nameof(route)} must be {nameof(ChoiceRouteNode)}");
        }

        public override IEnumerable<(string Key, string Value)> GetData(RouteMatchingStatus status)
        {
            var field = _selecter(status.Segment[0]);
            return field == null
                ? null
                : new[] { (field, status.Segment[0]) };
        }
    }
}
