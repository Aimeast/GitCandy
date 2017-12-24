using System.Collections.Generic;

namespace GitCandy.Router
{
    public class FieldRouteNode : RouteNode
    {
        private readonly string _field;

        public FieldRouteNode(string field)
        {
            _field = field;
        }

        public override int SkipSegment => 1;

        public override string FieldName => _field;

        public override IEnumerable<(string Key, string Value)> GetData(RouteMatchingStatus status)
        {
            return new[] { (_field, status.Segment[0]) };
        }
    }
}
