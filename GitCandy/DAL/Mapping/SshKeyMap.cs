using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace GitCandy.DAL.Mapping
{
    public class SshKeyMap : EntityTypeConfiguration<SshKey>
    {
        public SshKeyMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.KeyType)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.Fingerprint)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(47);

            this.Property(t => t.PublicKey)
                .IsRequired()
                .HasMaxLength(600);

            // Table & Column Mappings
            this.ToTable("SshKeys");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.UserID).HasColumnName("UserID");
            this.Property(t => t.KeyType).HasColumnName("KeyType");
            this.Property(t => t.Fingerprint).HasColumnName("Fingerprint");
            this.Property(t => t.PublicKey).HasColumnName("PublicKey");
            this.Property(t => t.ImportData).HasColumnName("ImportData");
            this.Property(t => t.LastUse).HasColumnName("LastUse");

            // Relationships
            this.HasRequired(t => t.User)
                .WithMany(t => t.SshKeys)
                .HasForeignKey(d => d.UserID);

        }
    }
}
