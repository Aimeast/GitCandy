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

        public LastCommitAccessor(string repoId, Repository repo, Commit commit, string path)
            : base(repoId, repo)
        {
            Contract.Requires(commit != null);
            Contract.Requires(path != null);
            Contract.Requires(commit[path] != null);

            this.commit = commit;
            this.path = path;

            var treeEntry = commit[path];
        }

        protected override string GetCacheKey()
        {
            return GetCacheKey(commit.Sha, path);
        }

        protected override void Init()
        {
            result = commit.Sha;
        }

        protected override void Calculate()
        {
            using (var repo = new Repository(this.repoPath))
            {
                var commit = repo.Lookup<Commit>(this.commit.Sha);
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
    }
}
