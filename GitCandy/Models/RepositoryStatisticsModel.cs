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
            public int Files { get; set; }
            public int Commits { get; set; }
            public int SourceSize { get; set; }
            public int Contributors { get; set; }
        }
    }
}