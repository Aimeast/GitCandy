using GitCandy.Git.Cache;
using LibGit2Sharp;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace GitCandy.Git
{
    public sealed class LastCommitAccessor : GitCacheAccessor<string, LastCommitAccessor>
    {
        private readonly Commit commit;
        private readonly string path;
        private readonly string key1, key2;

        public LastCommitAccessor(string repoId, Repository repo, Commit commit, string path)
            : base(repoId, repo)
        {
            Contract.Requires(commit != null);
            Contract.Requires(path != null);
            Contract.Requires(commit[path] != null);

            this.commit = commit;
            this.path = path;

            var treeEntry = commit[path];
            this.key1 = commit.Sha;
            this.key2 = treeEntry.Target.Sha;
        }

        protected override string GetCacheFile()
        {
            return GetCacheFile(key1, key2);
        }

        protected override void Init()
        {
            result = commit.Sha;
        }

        protected override void Calculate()
        {
            using (var repo = new Repository(this.repoPath))
            {
                var commit = repo.Lookup<Commit>(key1);
                var treeEntry = commit[path];
                if (treeEntry == null)
                {
                    resultDone = true;
                    return;
                }

                var gitObject = treeEntry.Target;
                var hs = new HashSet<string>();
                var queue = new Queue<Commit>();
                queue.Enqueue(commit);
                hs.Add(commit.Sha);
                while (queue.Count > 0)
                {
                    commit = queue.Dequeue();
                    result = commit.Sha;
                    var has = false;
                    foreach (var parent in commit.Parents)
                    {
                        treeEntry = parent[path];
                        if (treeEntry == null)
                            continue;
                        var eq = treeEntry.Target.Sha == gitObject.Sha;
                        if (eq && hs.Add(parent.Sha))
                            queue.Enqueue(parent);
                        has = has || eq;
                    }
                    if (!has)
                        break;
                }
                resultDone = true;
                return;
            }
        }

        public override bool Equals(object obj)
        {
            var accessor = obj as LastCommitAccessor;
            return accessor != null
                && repoId == accessor.repoId
                && key1 == accessor.key1
                && path == accessor.path;
        }

        public override int GetHashCode()
        {
            return typeof(LastCommitAccessor).GetHashCode() ^ (repoId + key1 + path).GetHashCode();
        }
    }
}
