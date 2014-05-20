using GitCandy.Extensions;
using GitCandy.Git.Cache;
using LibGit2Sharp;
using System.Diagnostics.Contracts;
using System.Linq;

namespace GitCandy.Git
{
    public class CommitsAccessor : GitCacheAccessor<RevisionSummaryCacheItem[], CommitsAccessor>
    {
        private readonly Commit commit;
        private readonly string path, key1, key2;
        private readonly int page, pageSize;

        public CommitsAccessor(string repoId, Repository repo, Commit commit, string path, int page, int pageSize)
            : base(repoId, repo)
        {
            Contract.Requires(commit != null);
            Contract.Requires(path != null);
            Contract.Requires(page >= 0);
            Contract.Requires(pageSize > 0);

            this.commit = commit;
            this.path = path;
            this.page = page;
            this.pageSize = pageSize;
            this.key1 = commit.Sha;
            this.key2 = path.Replace('/', ';');
        }

        public override bool IsAsync { get { return false; } }

        protected override string GetCacheFile()
        {
            return GetCacheFile(key1, key2, page, pageSize);
        }

        protected override void Init()
        {
            result = new RevisionSummaryCacheItem[0];
        }

        protected override void Calculate()
        {
            using (var repo = new Repository(this.repoPath))
            {
                result = repo.Commits
                    .QueryBy(new CommitFilter { Since = commit, SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time })
                    .PathFilter(path)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new RevisionSummaryCacheItem
                    {
                        CommitSha = s.Sha,
                        MessageShort = s.MessageShort.RepetitionIfEmpty(GitService.UnknowString),
                        AuthorName = s.Author.Name,
                        AuthorEmail = s.Author.Email,
                        AuthorWhen = s.Author.When,
                        CommitterName = s.Committer.Name,
                        CommitterEmail = s.Committer.Email,
                        CommitterWhen = s.Committer.When,
                    })
                    .ToArray();
            }
            resultDone = true;
        }

        public override bool Equals(object obj)
        {
            var accessor = obj as CommitsAccessor;
            return accessor != null
                && repoId == accessor.repoId
                && key1 == accessor.key1
                && path == accessor.path
                && page == accessor.page
                && pageSize == accessor.pageSize;
        }

        public override int GetHashCode()
        {
            return typeof(CommitsAccessor).GetHashCode() ^ (repoId + key1 + path + page + pageSize).GetHashCode();
        }
    }
}
