using GitCandy.Extensions;
using GitCandy.Git.Cache;
using GitCandy.Models;
using LibGit2Sharp;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace GitCandy.Git
{
    public class ScopeAccessor : GitCacheAccessor<RepositoryScope, ScopeAccessor>
    {
        private readonly Commit commit;
        private readonly string key1, key2, path;

        public ScopeAccessor(string repoId, Repository repo, Commit commit, string path = "")
            : base(repoId, repo)
        {
            Contract.Requires(commit != null);

            this.commit = commit;
            this.path = path;
            this.key1 = commit.Sha;
            var tree = commit[path];
            if (tree != null)
                this.key2 = tree.Target.Sha;
        }

        protected override string GetCacheFile()
        {
            return GetCacheFile(key1, key2);
        }

        protected override void Init()
        {
            result = new RepositoryScope
            {
                Commits = 0,
                Contributors = 0,
                Branches = repo.Branches.Count(),
                Tags = repo.Tags.Count(),
            };
        }

        protected override void Calculate()
        {
            using (var repo = new Repository(this.repoPath))
            {
                var ancestors = key2 == null
                    ? repo.Commits.QueryBy(new CommitFilter { Since = commit })
                    : repo.Commits.QueryBy(new CommitFilter { Since = commit }).PathFilter(path);

                var set = new HashSet<string>();
                foreach (var ancestor in ancestors)
                {
                    result.Commits++;
                    if (set.Add(ancestor.Author.ToString()))
                        result.Contributors++;
                }
            }
            resultDone = true;
        }

        public override bool Equals(object obj)
        {
            var accessor = obj as ScopeAccessor;
            return accessor != null
                && repoId == accessor.repoId
                && key1 == accessor.key1
                && key2 == accessor.key2;
        }

        public override int GetHashCode()
        {
            return typeof(ScopeAccessor).GetHashCode() ^ (repoId + key1 + key2).GetHashCode();
        }
    }
}
