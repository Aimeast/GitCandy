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
        private readonly string path;
        private readonly bool pathExist;

        public ScopeAccessor(string repoId, Repository repo, Commit commit, string path = "")
            : base(repoId, repo)
        {
            Contract.Requires(commit != null);

            this.commit = commit;
            this.path = path;
            this.pathExist = commit[path] != null;
        }

        protected override string GetCacheKey()
        {
            return GetCacheKey(commit.Sha, path);
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
                var ancestors = pathExist
                    ? repo.Commits.QueryBy(new CommitFilter { Since = commit }).PathFilter(path)
                    : repo.Commits.QueryBy(new CommitFilter { Since = commit });

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
    }
}
