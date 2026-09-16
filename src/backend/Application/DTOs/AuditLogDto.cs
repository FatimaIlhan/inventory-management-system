namespace Application.DTOs;

public sealed record AuditLogDto(
    long AuditLogId,
    long UserId,
    string UserEmail,
    string EntityType,
    long? EntityId,
    string Action,
    string Description,
    DateTime CreatedAtUtc);