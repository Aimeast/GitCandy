using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace GitCandy.DAL.Mapping
{
    public class RepositoryMap : EntityTypeConfiguration<Repository>
    {
        public RepositoryMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("Repositories");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
            this.Property(t => t.IsPrivate).HasColumnName("IsPrivate");
            this.Property(t => t.AllowAnonymousRead).HasColumnName("AllowAnonymousRead");
            this.Property(t => t.AllowAnonymousWrite).HasColumnName("AllowAnonymousWrite");
        }
    }
}
