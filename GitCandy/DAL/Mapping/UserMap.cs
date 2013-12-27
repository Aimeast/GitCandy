using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace GitCandy.DAL.Mapping
{
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.Nickname)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.Email)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.Password)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(32);

            this.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("Users");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Nickname).HasColumnName("Nickname");
            this.Property(t => t.Email).HasColumnName("Email");
            this.Property(t => t.PasswordVersion).HasColumnName("PasswordVersion");
            this.Property(t => t.Password).HasColumnName("Password");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.IsSystemAdministrator).HasColumnName("IsSystemAdministrator");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
        }
    }
}
