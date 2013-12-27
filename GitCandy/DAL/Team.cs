using System;
using System.Collections.Generic;

namespace GitCandy.DAL
{
    public partial class Team
    {
        public Team()
        {
            this.TeamRepositoryRoles = new List<TeamRepositoryRole>();
            this.UserTeamRoles = new List<UserTeamRole>();
        }

        public long ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public System.DateTime CreationDate { get; set; }
        public virtual ICollection<TeamRepositoryRole> TeamRepositoryRoles { get; set; }
        public virtual ICollection<UserTeamRole> UserTeamRoles { get; set; }
    }
}
