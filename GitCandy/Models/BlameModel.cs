
namespace GitCandy.Models
{
    public class BlameModel : RepositoryModelBase
    {
        public string ReferenceName { get; set; }
        public string Path { get; set; }
        public string Sha { get; set; }
        public BlameHunkModel[] Hunks { get; set; }
        public string Brush { get; set; }
        public PathBarModel PathBar { get; set; }
        public string SizeString { get; set; }
    }
}