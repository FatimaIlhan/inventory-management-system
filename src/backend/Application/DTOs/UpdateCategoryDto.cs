namespace Application.DTOs;

public sealed record UpdateCategoryDto(
    string Name,
    string? Description);
