using System;

namespace GitCandy.Models
{
    public class RepositoryStatisticsModel
    {
        public Statistics Default { get; set; }
        public Statistics Current { get; set; }
        public long RepositorySize { get; set; }

        [Serializable]
        public class Statistics
        {
            public string Branch { get; set; }
            public int NumberOfFiles { get; set; }
            public int NumberOfCommits { get; set; }
            public int SizeOfSource { get; set; }
            public int NumberOfContributors { get; set; }
            public ContributorCommits[] OrderedCommits { get; set; }
        }

        [Serializable]
        public class ContributorCommits
        {
            public string Author { get; set; }
            public int CommitsCount { get; set; }
        }
    }
}