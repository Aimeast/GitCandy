namespace GitCandy.Models
{
    public class TagsModel : RepositoryModelBase
    {
        public TagModel[] Tags { get; set; }
        public bool HasTags { get { return Tags != null && Tags.Length != 0; } }

        public bool CanDelete { get; set; }
    }
}