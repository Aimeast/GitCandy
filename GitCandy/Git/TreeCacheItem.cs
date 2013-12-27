using System;

namespace GitCandy.Git
{
    [Serializable]
    public class TreeCacheItem
    {
        public RevisionSummaryCacheItem[] RevisionSummary;
        public int CommitsCount;
        public int ContributorsCount;
    }
}