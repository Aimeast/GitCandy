using System.Collections.Generic;

namespace GitCandy.Router
{
    public class ChoiceRouteNode : RouteNode
    {
        private readonly string _field;

        public ChoiceRouteNode(string field)
        {
            _field = field;
        }

        public override int SkipSegment => 0;

        public override string FieldName => _field;

        public override IEnumerable<(string Key, string Value)> GetData(RouteMatchingStatus status)
        {
            return status.RouteData.Keys.Contains(_field)
                ? EmptyRouteData
                : null;
        }
    }
}
