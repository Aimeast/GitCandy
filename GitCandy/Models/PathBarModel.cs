
namespace GitCandy.Models
{
    public class PathBarModel
    {
        public string Name { get; set; }
        public string ReferenceName { get; set; }
        public string ReferenceSha { get; set; }
        public string Path { get; set; }
        public string Action { get; set; }
        public bool HideLastSlash { get; set; }
    }
}