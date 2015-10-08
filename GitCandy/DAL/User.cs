using System;
using System.Collections.Generic;

namespace GitCandy.DAL
{
    public partial class User
    {
        public User()
        {
            this.AuthorizationLogs = new List<AuthorizationLog>();
            this.SshKeys = new List<SshKey>();
            this.UserRepositoryRoles = new List<UserRepositoryRole>();
            this.UserTeamRoles = new List<UserTeamRole>();
        }

        public long ID { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public int PasswordVersion { get; set; }
        public string Password { get; set; }
        public string Description { get; set; }
        public bool IsSystemAdministrator { get; set; }
        public System.DateTime CreationDate { get; set; }
        public virtual ICollection<AuthorizationLog> AuthorizationLogs { get; set; }
        public virtual ICollection<SshKey> SshKeys { get; set; }
        public virtual ICollection<UserRepositoryRole> UserRepositoryRoles { get; set; }
        public virtual ICollection<UserTeamRole> UserTeamRoles { get; set; }
    }
}
