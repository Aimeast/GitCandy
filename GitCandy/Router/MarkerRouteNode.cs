using System;
using System.Collections.Generic;

namespace GitCandy.Router
{
    public class MarkerRouteNode : RouteNode
    {
        private readonly string _field;
        private readonly Func<string, bool> _selecter;

        public MarkerRouteNode(string field, Func<string, bool> selecter)
        {
            _field = field;
            _selecter = selecter;
        }

        public override int SkipSegment => 0;

        public override string FieldName => "_" + _field;

        public override IEnumerable<(string Key, string Value)> GetData(RouteMatchingStatus status)
        {
            return _selecter(status.Segment[0])
               ? new[] { (FieldName, status.Segment[0]) }
               : null;
        }
    }
}
