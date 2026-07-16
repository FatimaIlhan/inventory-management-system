namespace Application.DTOs;

public sealed record AuthenticatedUserDto(
    long Id,
    string Email,
    string Role);
