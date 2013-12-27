using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using GitCandy.DAL.Mapping;

namespace GitCandy.DAL
{
    public partial class GitCandyContext : DbContext
    {
        static GitCandyContext()
        {
            Database.SetInitializer<GitCandyContext>(null);
        }

        public GitCandyContext()
            : base("Name=GitCandyContext")
        {
        }

        public DbSet<AuthorizationLog> AuthorizationLogs { get; set; }
        public DbSet<Repository> Repositories { get; set; }
        public DbSet<TeamRepositoryRole> TeamRepositoryRoles { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<UserRepositoryRole> UserRepositoryRoles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserTeamRole> UserTeamRoles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new AuthorizationLogMap());
            modelBuilder.Configurations.Add(new RepositoryMap());
            modelBuilder.Configurations.Add(new TeamRepositoryRoleMap());
            modelBuilder.Configurations.Add(new TeamMap());
            modelBuilder.Configurations.Add(new UserRepositoryRoleMap());
            modelBuilder.Configurations.Add(new UserMap());
            modelBuilder.Configurations.Add(new UserTeamRoleMap());
        }
    }
}
