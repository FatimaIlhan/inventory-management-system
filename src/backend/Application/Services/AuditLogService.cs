using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public sealed class AuditLogService(
    IAuditLogRepository auditLogRepository,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider) : IAuditLogService
{
    public Task RecordAsync(string entityType, long? entityId, string action, string description, CancellationToken cancellationToken)
    {
        var auditLog = new AuditLog
        {
            UserId = currentUserService.UserId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Description = description,
            CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime
        };

        return auditLogRepository.CreateAsync(auditLog, cancellationToken);
    }

    public async Task<PagedResultDto<AuditLogDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? entityType,
        string? action,
        long? userId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        if (page < 1)
        {
            throw new AppValidationException("Page must be greater than or equal to 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new AppValidationException("Page size must be between 1 and 100.");
        }

        if (fromUtc.HasValue && toUtc.HasValue && fromUtc > toUtc)
        {
            throw new AppValidationException("The start date must be before or equal to the end date.");
        }

        var (items, totalCount) = await auditLogRepository.GetPagedAsync(
            page, pageSize, search, entityType, action, userId, fromUtc, toUtc, cancellationToken);

        return new PagedResultDto<AuditLogDto>(
            items.Select(item => new AuditLogDto(
                item.AuditLogId,
                item.UserId,
                item.User.Email ?? string.Empty,
                item.EntityType,
                item.EntityId,
                item.Action,
                item.Description,
                item.CreatedAtUtc)).ToList(),
            page,
            pageSize,
            totalCount);
    }
}