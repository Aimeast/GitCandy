namespace GitCandy.Models
{
    public class RepositoryListModel
    {
        public RepositoryModel[] Collaborations { get; set; }
        public RepositoryModel[] Repositories { get; set; }
        public bool CanCreateRepository { get; set; }
    }
}