namespace Domain.Entities;

public sealed class AuditLog
{
    public long AuditLogId { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public string EntityType { get; set; } = string.Empty;
    public long? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}