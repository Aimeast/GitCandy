using LibGit2Sharp;
using System.Text;

namespace GitCandy.Models
{
    public class TreeEntryModel : RepositoryModelBase
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string ReferenceName { get; set; }
        public CommitModel Commit { get; set; }
        public string Sha { get; set; }
        public TreeEntryTargetType EntryType { get; set; }
        public byte[] RawData { get; set; }
        public string SizeString { get; set; }
        public string TextContent { get; set; }
        public string TextBrush { get; set; }
        public BlobType BlobType { get; set; }
        public Encoding BlobEncoding { get; set; }
        public PathBarModel PathBar { get; set; }
    }
}