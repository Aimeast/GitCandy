namespace GitCandy.Models
{
    public abstract class RepositoryModelBase
    {
        public string RepositoryName { get; set; }
        public BranchSelectorModel BranchSelector { get; set; }
    }
}