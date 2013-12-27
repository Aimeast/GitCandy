using System;

namespace GitCandy.Git
{
    [Serializable]
    public class RevisionSummaryCacheItem
    {
        public string Name;
        public string Sha;
        public string MessageShort;
        public string AuthorName;
        public string AuthorEmail;
        public DateTimeOffset AuthorWhen;
        public string CommitterName;
        public string CommitterEmail;
        public DateTimeOffset CommitterWhen;
        public int Ahead;
        public int Behind;
    }
}