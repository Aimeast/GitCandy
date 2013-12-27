using System.Collections.Generic;

namespace GitCandy.Models
{
    public class TreeModel : RepositoryModelBase
    {
        public string ReferenceName { get; set; }
        public string Path { get; set; }
        public CommitModel Commit { get; set; }
        public IEnumerable<TreeEntryModel> Entries { get; set; }
        public TreeEntryModel Readme { get; set; }
        public bool IsRoot { get { return string.IsNullOrEmpty(Path) || Path == "\\" || Path == "/"; } }
        public RepositoryScope Scope { get; set; }
        public string GitUrl { get; set; }
        public string Description { get; set; }
        public PathBarModel PathBar { get; set; }
    }
}