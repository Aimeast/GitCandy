using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace GitCandy.DAL.Mapping
{
    public class AuthorizationLogMap : EntityTypeConfiguration<AuthorizationLog>
    {
        public AuthorizationLogMap()
        {
            // Primary Key
            this.HasKey(t => t.AuthCode);

            // Properties
            this.Property(t => t.IssueIp)
                .IsRequired()
                .HasMaxLength(40);

            this.Property(t => t.LastIp)
                .IsRequired()
                .HasMaxLength(40);

            // Table & Column Mappings
            this.ToTable("AuthorizationLog");
            this.Property(t => t.AuthCode).HasColumnName("AuthCode");
            this.Property(t => t.UserID).HasColumnName("UserID");
            this.Property(t => t.IssueDate).HasColumnName("IssueDate");
            this.Property(t => t.Expires).HasColumnName("Expires");
            this.Property(t => t.IssueIp).HasColumnName("IssueIp");
            this.Property(t => t.LastIp).HasColumnName("LastIp");
            this.Property(t => t.IsValid).HasColumnName("IsValid");

            // Relationships
            this.HasRequired(t => t.User)
                .WithMany(t => t.AuthorizationLogs)
                .HasForeignKey(d => d.UserID);

        }
    }
}
