namespace Api.DTOs;

public sealed record ApiErrorResponse(
    string TraceId,
    string Code,
    string Message,
    IReadOnlyCollection<string>? Details = null);