using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class AuditLogRepository(InventoryDbContext dbContext) : IAuditLogRepository
{
    public async Task CreateAsync(AuditLog auditLog, CancellationToken cancellationToken)
    {
        await dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, string? entityType, string? action, long? userId,
        DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken)
    {
        var query = dbContext.AuditLogs.Include(auditLog => auditLog.User).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();
            query = query.Where(auditLog => auditLog.Description.ToLower().Contains(value) || auditLog.User.Email!.ToLower().Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(entityType)) query = query.Where(auditLog => auditLog.EntityType == entityType.Trim());
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(auditLog => auditLog.Action == action.Trim());
        if (userId.HasValue) query = query.Where(auditLog => auditLog.UserId == userId.Value);
        if (fromUtc.HasValue) query = query.Where(auditLog => auditLog.CreatedAtUtc >= fromUtc.Value);
        if (toUtc.HasValue) query = query.Where(auditLog => auditLog.CreatedAtUtc <= toUtc.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(auditLog => auditLog.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}