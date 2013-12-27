using LibGit2Sharp;
using System.Collections.Generic;

namespace GitCandy.Models
{
    public class CommitModel : RepositoryModelBase
    {
        public string Sha { get; set; }
        public string ReferenceName { get; set; }
        public string CommitMessageShort { get; set; }
        public string CommitMessage { get; set; }
        public Signature Author { get; set; }
        public Signature Committer { get; set; }
        public string[] Parents { get; set; }
        public IEnumerable<CommitChangeModel> Changes { get; set; }
        public PathBarModel PathBar { get; set; }
    }
}