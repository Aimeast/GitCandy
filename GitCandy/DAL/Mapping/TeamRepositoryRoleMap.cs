using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace GitCandy.DAL.Mapping
{
    public class TeamRepositoryRoleMap : EntityTypeConfiguration<TeamRepositoryRole>
    {
        public TeamRepositoryRoleMap()
        {
            // Primary Key
            this.HasKey(t => new { t.TeamID, t.RepositoryID });

            // Properties
            this.Property(t => t.TeamID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.RepositoryID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("TeamRepositoryRole");
            this.Property(t => t.TeamID).HasColumnName("TeamID");
            this.Property(t => t.RepositoryID).HasColumnName("RepositoryID");
            this.Property(t => t.AllowRead).HasColumnName("AllowRead");
            this.Property(t => t.AllowWrite).HasColumnName("AllowWrite");

            // Relationships
            this.HasRequired(t => t.Repository)
                .WithMany(t => t.TeamRepositoryRoles)
                .HasForeignKey(d => d.RepositoryID);
            this.HasRequired(t => t.Team)
                .WithMany(t => t.TeamRepositoryRoles)
                .HasForeignKey(d => d.TeamID);

        }
    }
}
