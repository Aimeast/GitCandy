
namespace GitCandy.Models
{
    public class TeamListModel
    {
        public TeamModel[] Teams { get; set; }
        public int CurrentPage { get; set; }
        public int ItemCount { get; set; }
    }
}