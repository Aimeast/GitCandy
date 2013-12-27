using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace GitCandy.DAL.Mapping
{
    public class UserTeamRoleMap : EntityTypeConfiguration<UserTeamRole>
    {
        public UserTeamRoleMap()
        {
            // Primary Key
            this.HasKey(t => new { t.UserID, t.TeamID });

            // Properties
            this.Property(t => t.UserID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.TeamID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("UserTeamRole");
            this.Property(t => t.UserID).HasColumnName("UserID");
            this.Property(t => t.TeamID).HasColumnName("TeamID");
            this.Property(t => t.IsAdministrator).HasColumnName("IsAdministrator");

            // Relationships
            this.HasRequired(t => t.Team)
                .WithMany(t => t.UserTeamRoles)
                .HasForeignKey(d => d.TeamID);
            this.HasRequired(t => t.User)
                .WithMany(t => t.UserTeamRoles)
                .HasForeignKey(d => d.UserID);

        }
    }
}
