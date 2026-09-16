using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(auditLog => auditLog.AuditLogId);
        builder.Property(auditLog => auditLog.AuditLogId).HasColumnName("audit_log_id");
        builder.Property(auditLog => auditLog.UserId).HasColumnName("user_id").IsRequired();
        builder.HasOne(auditLog => auditLog.User).WithMany().HasForeignKey(auditLog => auditLog.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(auditLog => auditLog.EntityType).HasColumnName("entity_type").HasMaxLength(50).IsRequired();
        builder.Property(auditLog => auditLog.EntityId).HasColumnName("entity_id");
        builder.Property(auditLog => auditLog.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
        builder.Property(auditLog => auditLog.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        builder.Property(auditLog => auditLog.CreatedAtUtc).HasColumnName("created_at").IsRequired();
        builder.HasIndex(auditLog => auditLog.CreatedAtUtc);
        builder.HasIndex(auditLog => new { auditLog.EntityType, auditLog.Action });
        builder.HasIndex(auditLog => auditLog.UserId);
    }
}