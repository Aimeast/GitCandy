using LibGit2Sharp;
using System.Collections.Generic;
using System.Linq;

namespace GitCandy.Extensions
{
    public static class CommitLogExtension
    {
        public static IEnumerable<Commit> PathFilter(this IEnumerable<Commit> log, string path)
        {
            if (string.IsNullOrEmpty(path))
                return log;

            return log.Where(s =>
            {
                var pathEntry = s[path];
                var parent = s.Parents.FirstOrDefault();
                if (parent == null)
                    return pathEntry != null;

                var parentPathEntry = parent[path];
                if (pathEntry == null && parentPathEntry == null)
                    return false;
                if (pathEntry != null && parentPathEntry != null)
                    return pathEntry.Target.Sha != parentPathEntry.Target.Sha;
                return true;
            });
        }
    }
}