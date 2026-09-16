using Application.DTOs;

namespace Application.Interfaces;

public interface IAuditLogService
{
    Task RecordAsync(string entityType, long? entityId, string action, string description, CancellationToken cancellationToken);

    Task<PagedResultDto<AuditLogDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? entityType,
        string? action,
        long? userId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken);
}