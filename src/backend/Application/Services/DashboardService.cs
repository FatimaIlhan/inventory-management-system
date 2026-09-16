using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;

namespace Application.Services;

public sealed class DashboardService(
    IDashboardRepository dashboardRepository,
    TimeProvider timeProvider) : IDashboardService
{
    public Task<DashboardSummaryDto> GetSummaryAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        var todayUtc = timeProvider.GetUtcNow().UtcDateTime.Date;
        var normalizedFromUtc = (fromUtc ?? todayUtc.AddDays(-29)).Date;
        var normalizedToUtc = (toUtc ?? todayUtc).Date;

        if (normalizedToUtc < normalizedFromUtc)
        {
            throw new AppValidationException("The end date must be on or after the start date.");
        }

        return dashboardRepository.GetSummaryAsync(
            normalizedFromUtc,
            normalizedToUtc.AddDays(1),
            cancellationToken);
    }
}