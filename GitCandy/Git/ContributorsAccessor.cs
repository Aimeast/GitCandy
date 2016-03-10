using GitCandy.Git.Cache;
using GitCandy.Models;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace GitCandy.Git
{
    public class ContributorsAccessor : GitCacheAccessor<RepositoryStatisticsModel.Statistics, ContributorsAccessor>
    {
        private Commit commit;
        private string key;

        public ContributorsAccessor(string repoId, Repository repo, Commit commit)
            : base(repoId, repo)
        {
            Contract.Requires(commit != null);

            this.commit = commit;
            this.key = commit.Sha;
        }

        protected override string GetCacheKey()
        {
            return GetCacheKey(key);
        }

        protected override void Init()
        {
            result = new RepositoryStatisticsModel.Statistics
            {
                OrderedCommits = new RepositoryStatisticsModel.ContributorCommits[0]
            };
        }

        protected override void Calculate()
        {
            using (var repo = new Repository(this.repoPath))
            {
                var commit = repo.Lookup<Commit>(key);
                var ancestors = repo.Commits
                    .QueryBy(new CommitFilter { IncludeReachableFrom = commit });

                var dict = new Dictionary<string, int>();
                var statistics = new RepositoryStatisticsModel.Statistics();
                foreach (var ancestor in ancestors)
                {
                    statistics.NumberOfCommits++;
                    var author = ancestor.Author.ToString();
                    if (dict.ContainsKey(author))
                    {
                        dict[author]++;
                    }
                    else
                    {
                        dict.Add(author, 1);
                        statistics.NumberOfContributors++;
                    }
                }
                var size = 0L;
                statistics.NumberOfFiles = FilesInCommit(commit, out size);
                statistics.SizeOfSource = size;

                var commits = dict
                    .OrderByDescending(s => s.Value)
                    .Select(s => new RepositoryStatisticsModel.ContributorCommits { Author = s.Key, CommitsCount = s.Value })
                    .ToArray();

                statistics.OrderedCommits = commits;

                result = statistics;
                resultDone = true;
            }
        }

        private int FilesInCommit(Commit commit, out long sourceSize)
        {
            var count = 0;
            var stack = new Stack<Tree>();
            sourceSize = 0;

            var repo = ((IBelongToARepository)commit).Repository;

            stack.Push(commit.Tree);
            while (stack.Count != 0)
            {
                var tree = stack.Pop();
                foreach (var entry in tree)
                    switch (entry.TargetType)
                    {
                        case TreeEntryTargetType.Blob:
                            count++;
                            sourceSize += repo.ObjectDatabase.RetrieveObjectMetadata(entry.Target.Id).Size;
                            break;
                        case TreeEntryTargetType.Tree:
                            stack.Push((Tree)entry.Target);
                            break;
                    }
            }
            return count;
        }
    }
}
