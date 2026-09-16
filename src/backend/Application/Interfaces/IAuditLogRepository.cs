using Domain.Entities;

namespace Application.Interfaces;

public interface IAuditLogRepository
{
    Task CreateAsync(AuditLog auditLog, CancellationToken cancellationToken);

    Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(
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