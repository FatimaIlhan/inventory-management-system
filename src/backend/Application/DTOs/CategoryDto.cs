namespace Application.DTOs;

public sealed record CategoryDto(
    long Id,
    string Name,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
