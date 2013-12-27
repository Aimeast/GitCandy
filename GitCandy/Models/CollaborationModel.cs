namespace GitCandy.Models
{
    public class CollaborationModel : RepositoryModelBase
    {
        public UserRole[] Users { get; set; }
        public TeamRole[] Teams { get; set; }

        public class UserRole
        {
            public string Name { get; set; }
            public bool AllowRead { get; set; }
            public bool AllowWrite { get; set; }
            public bool IsOwner { get; set; }
        }

        public class TeamRole
        {
            public string Name { get; set; }
            public bool AllowRead { get; set; }
            public bool AllowWrite { get; set; }
        }
    }
}