using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace GitCandy.DAL.Mapping
{
    public class UserRepositoryRoleMap : EntityTypeConfiguration<UserRepositoryRole>
    {
        public UserRepositoryRoleMap()
        {
            // Primary Key
            this.HasKey(t => new { t.UserID, t.RepositoryID });

            // Properties
            this.Property(t => t.UserID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.RepositoryID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("UserRepositoryRole");
            this.Property(t => t.UserID).HasColumnName("UserID");
            this.Property(t => t.RepositoryID).HasColumnName("RepositoryID");
            this.Property(t => t.AllowRead).HasColumnName("AllowRead");
            this.Property(t => t.AllowWrite).HasColumnName("AllowWrite");
            this.Property(t => t.IsOwner).HasColumnName("IsOwner");

            // Relationships
            this.HasRequired(t => t.Repository)
                .WithMany(t => t.UserRepositoryRoles)
                .HasForeignKey(d => d.RepositoryID);
            this.HasRequired(t => t.User)
                .WithMany(t => t.UserRepositoryRoles)
                .HasForeignKey(d => d.UserID);

        }
    }
}
