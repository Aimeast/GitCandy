using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GitCandy.Router
{
    [DebuggerDisplay(nameof(NodeMatcher) + " -> {_status.RouteNode.GetType().Name}")]
    public class NodeMatcher
    {
        private readonly RouteMatchingStatus _status;

        public NodeMatcher(RouteMatchingStatus status)
        {
            _status = status;
        }

        public NodeMatcher Parent { get; private set; }
        public IDictionary<string, string> RouteData => _status.RouteData;
        public bool Succeed { get; private set; }

        public bool Match()
        {
            var isMarkerRoute = _status.RouteNode is MarkerRouteNode;
            _status.SkipSegment = _status.RouteNode.SkipSegment;

            if (_status.Segment.Length == 0)
                return isMarkerRoute;

            var pairs = _status.RouteNode.GetData(_status);
            if (pairs == null)
                return isMarkerRoute;

            if (isMarkerRoute)
                _status.SkipSegment++;

            foreach (var (key, value) in pairs)
            {
                _status.RouteData.Add(key, value);
            }
            Succeed = _status.RouteNode is ActionRouteNode;

            return true;
        }

        public IEnumerable<NodeMatcher> GetChildren()
        {
            foreach (var tree in _status.RouteNode.Children)
            {
                var status = new RouteMatchingStatus(tree,
                    _status.Method,
                    _status.Segment.Slice(_status.SkipSegment),
                    _status.Query,
                    new Dictionary<string, string>(_status.RouteData, StringComparer.OrdinalIgnoreCase));
                yield return new NodeMatcher(status) { Parent = this };
            }
        }
    }
}
