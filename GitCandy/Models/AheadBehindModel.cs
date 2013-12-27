
namespace GitCandy.Models
{
    public class AheadBehindModel
    {
        public int Ahead { get; set; }
        public int Behind { get; set; }
        public CommitModel Commit { get; set; }
    }
}