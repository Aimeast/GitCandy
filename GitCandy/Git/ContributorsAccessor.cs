using GitCandy.Git.Cache;
using GitCandy.Models;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace GitCandy.Git
{
    public class ContributorsAccessor : GitCacheAccessor<Tuple<ContributorCommitsModel[], RepositoryStatisticsModel.Statistics>, ContributorsAccessor>
    {
        private Commit commit;
        private string key;
        private int numbers;

        public ContributorsAccessor(string repoId, Repository repo, Commit commit, int numbersOfTopContributors)
            : base(repoId, repo)
        {
            Contract.Requires(commit != null);
            Contract.Requires(numbersOfTopContributors > 0);

            this.commit = commit;
            this.key = commit.Sha;
            this.numbers = numbersOfTopContributors;
        }

        protected override string GetCacheFile()
        {
            return GetCacheFile(key, numbers);
        }

        protected override void Init()
        {
            result = Tuple.Create(new ContributorCommitsModel[0], new RepositoryStatisticsModel.Statistics());
        }

        protected override void Calculate()
        {
            using (var repo = new Repository(this.repoPath))
            {
                var commit = repo.Lookup<Commit>(key);
                var ancestors = repo.Commits
                    .QueryBy(new CommitFilter { Since = commit });

                var dict = new Dictionary<string, int>();
                var statistics = new RepositoryStatisticsModel.Statistics();
                foreach (var ancestor in ancestors)
                {
                    statistics.Commits++;
                    var author = ancestor.Author.ToString();
                    if (dict.ContainsKey(author))
                    {
                        dict[author]++;
                    }
                    else
                    {
                        dict.Add(author, 1);
                        statistics.Contributors++;
                    }
                }
                long size = 0;
                statistics.Files = FilesInCommit(commit, out size);
                statistics.SourceSize = size;

                var topN = dict
                    .OrderByDescending(s => s.Value)
                    .Select(s => new ContributorCommitsModel { Author = s.Key, CommitsCount = s.Value })
                    .Take(numbers)
                    .ToArray();

                result = Tuple.Create(topN, statistics);
                resultDone = true;
            }
        }

        private int FilesInCommit(Commit commit, out long sourceSize)
        {
            var count = 0;
            var stack = new Stack<Tree>();
            sourceSize = 0;

            stack.Push(commit.Tree);
            while (stack.Count != 0)
            {
                var tree = stack.Pop();
                foreach (var entry in tree)
                    switch (entry.TargetType)
                    {
                        case TreeEntryTargetType.Blob:
                            count++;
                            sourceSize += ((Blob)entry.Target).Size;
                            break;
                        case TreeEntryTargetType.Tree:
                            stack.Push((Tree)entry.Target);
                            break;
                    }
            }
            return count;
        }

        public override bool Equals(object obj)
        {
            var accessor = obj as ContributorsAccessor;
            return accessor != null
                && repoId == accessor.repoId
                && key == accessor.key;
        }

        public override int GetHashCode()
        {
            return typeof(ContributorsAccessor).GetHashCode() ^ (repoId + key).GetHashCode();
        }
    }
}
