using GitCandy.Git.Cache;
using LibGit2Sharp;
using System.Diagnostics.Contracts;
using System.Linq;

namespace GitCandy.Git
{
    public class HistoryDivergenceAccessor : GitCacheAccessor<RevisionSummaryCacheItem[], HistoryDivergenceAccessor>
    {
        private readonly string branchesSha;

        public HistoryDivergenceAccessor(string repoId, Repository repo, string branchesSha)
            : base(repoId, repo)
        {
            Contract.Requires(branchesSha != null);

            this.branchesSha = branchesSha;
        }

        protected override string GetCacheFile()
        {
            return GetCacheFile(branchesSha);
        }

        protected override void Init()
        {
            var head = repo.Head;
            if (head.Tip == null)
                return;

            result = repo.Branches
                .Where(s => s != head && s.Name != "HEAD")
                .OrderByDescending(s => s.Tip.Author.When)
                .Select(branch =>
                {
                    var commit = branch.Tip;
                    return new RevisionSummaryCacheItem
                    {
                        Ahead = 0,
                        Behind = 0,
                        Name = branch.Name,
                        CommitSha = commit.Sha,
                        AuthorName = commit.Author.Name,
                        AuthorEmail = commit.Author.Email,
                        AuthorWhen = commit.Author.When,
                        CommitterName = commit.Committer.Name,
                        CommitterEmail = commit.Committer.Email,
                        CommitterWhen = commit.Committer.When,
                    };
                })
                .ToArray();
        }

        protected override void Calculate()
        {
            using (var repo = new Repository(this.repoPath))
            {
                var head = repo.Head;
                foreach (var item in result)
                {
                    var commit = repo.Branches[item.Name].Tip;
                    var divergence = repo.ObjectDatabase.CalculateHistoryDivergence(commit, head.Tip);
                    item.Ahead = divergence.AheadBy ?? 0;
                    item.Behind = divergence.BehindBy ?? 0;
                }
            }
            resultDone = true;
        }

        public override bool Equals(object obj)
        {
            var accessor = obj as HistoryDivergenceAccessor;
            return accessor != null
                && repoId == accessor.repoId
                && branchesSha == accessor.branchesSha;
        }

        public override int GetHashCode()
        {
            return typeof(HistoryDivergenceAccessor).GetHashCode() ^ (repoId + branchesSha).GetHashCode();
        }
    }
}
