using System;
using System.Collections.Generic;

namespace GitCandy.DAL
{
    public partial class Repository
    {
        public Repository()
        {
            this.TeamRepositoryRoles = new List<TeamRepositoryRole>();
            this.UserRepositoryRoles = new List<UserRepositoryRole>();
        }

        public long ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public System.DateTime CreationDate { get; set; }
        public bool IsPrivate { get; set; }
        public bool AllowAnonymousRead { get; set; }
        public bool AllowAnonymousWrite { get; set; }
        public virtual ICollection<TeamRepositoryRole> TeamRepositoryRoles { get; set; }
        public virtual ICollection<UserRepositoryRole> UserRepositoryRoles { get; set; }
    }
}
