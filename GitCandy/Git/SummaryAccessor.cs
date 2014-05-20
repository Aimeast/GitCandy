using GitCandy.Base;
using GitCandy.Extensions;
using GitCandy.Git.Cache;
using LibGit2Sharp;
using System.Diagnostics.Contracts;
using System.Linq;

namespace GitCandy.Git
{
    public class SummaryAccessor : GitCacheAccessor<RevisionSummaryCacheItem[], SummaryAccessor>
    {
        private readonly Commit commit;
        private readonly Tree tree;
        private readonly string key;

        public SummaryAccessor(string repoId, Repository repo, Commit commit, Tree tree)
            : base(repoId, repo)
        {
            Contract.Requires(commit != null);
            Contract.Requires(tree != null);

            this.commit = commit;
            this.tree = tree;
            this.key = tree.Sha;
        }

        protected override string GetCacheFile()
        {
            return GetCacheFile(key);
        }

        protected override void Init()
        {
            result = tree
                .OrderBy(s => s.TargetType == TreeEntryTargetType.Blob)
                .ThenBy(s => s.Name, new StringLogicalComparer())
                .Select(s => new RevisionSummaryCacheItem
                {
                    Name = s.Name,
                    Path = s.Path.Replace('\\', '/'),
                    TargetSha = s.Target.Sha,
                    MessageShort = "Loading...",
                    AuthorEmail = "Loading...",
                    AuthorName = "Loading...",
                    CommitterEmail = "Loading...",
                    CommitterName = "Loading...",
                })
                .ToArray();
        }

        protected override void Calculate()
        {
            using (var repo = new Repository(this.repoPath))
            {
                var ancestors = repo.Commits
                    .QueryBy(new CommitFilter { Since = commit, SortBy = CommitSortStrategies.Topological });

                // null, continue search current reference
                // true, have found, done
                // false, search has been interrupted, but waiting for next match
                var status = new bool?[result.Length];
                var done = result.Length;
                Commit lastCommit = null;
                foreach (var ancestor in ancestors)
                {
                    for (var index = 0; index < result.Length; index++)
                    {
                        if (status[index] == true)
                            continue;
                        var item = result[index];
                        var ancestorEntry = ancestor[item.Path];
                        if (ancestorEntry != null && ancestorEntry.Target.Sha == item.TargetSha)
                        {
                            item.CommitSha = ancestor.Sha;
                            item.MessageShort = ancestor.MessageShort.RepetitionIfEmpty(GitService.UnknowString);
                            item.AuthorEmail = ancestor.Author.Email;
                            item.AuthorName = ancestor.Author.Name;
                            item.AuthorWhen = ancestor.Author.When;
                            item.CommitterEmail = ancestor.Committer.Email;
                            item.CommitterName = ancestor.Committer.Name;
                            item.CommitterWhen = ancestor.Committer.When;

                            status[index] = null;
                        }
                        else if (status[index] == null)
                        {
                            var over = true;
                            foreach (var parent in lastCommit.Parents) // Backtracking
                            {
                                if (parent.Sha == ancestor.Sha)
                                    continue;
                                var entry = parent[item.Path];
                                if (entry != null && entry.Target.Sha == item.TargetSha)
                                {
                                    over = false;
                                    break;
                                }
                            }
                            status[index] = over;
                            if (over)
                                done--;
                        }
                    }
                    if (done == 0)
                        break;
                    lastCommit = ancestor;
                }
            }
            resultDone = true;
        }

        public override bool Equals(object obj)
        {
            var accessor = obj as SummaryAccessor;
            return accessor != null
                && repoId == accessor.repoId
                && key == accessor.key;
        }

        public override int GetHashCode()
        {
            return typeof(SummaryAccessor).GetHashCode() ^ (repoId + key).GetHashCode();
        }
    }
}
