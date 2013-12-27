using LibGit2Sharp;

namespace GitCandy.Models
{
    public class CommitChangeModel
    {
        public string OldPath { get; set; }
        public string Path { get; set; }
        public ChangeKind ChangeKind { get; set; }
        public int LinesAdded { get; set; }
        public int LinesDeleted { get; set; }
        public string Patch { get; set; }
    }
}