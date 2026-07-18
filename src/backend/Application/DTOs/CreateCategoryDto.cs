namespace Application.DTOs;

public sealed record CreateCategoryDto(
    string Name,
    string? Description);
