using System.Collections.Generic;

namespace GitCandy.Models
{
    public class CommitsModel : RepositoryModelBase
    {
        public string ReferenceName { get; set; }
        public string Sha { get; set; }
        public string Path { get; set; }
        public IEnumerable<CommitModel> Commits { get; set; }
        public int CurrentPage { get; set; }
        public int ItemCount { get; set; }
        public PathBarModel PathBar { get; set; }
    }
}