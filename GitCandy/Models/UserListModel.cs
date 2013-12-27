
namespace GitCandy.Models
{
    public class UserListModel
    {
        public UserModel[] Users { get; set; }
        public int CurrentPage { get; set; }
        public int ItemCount { get; set; }
    }
}