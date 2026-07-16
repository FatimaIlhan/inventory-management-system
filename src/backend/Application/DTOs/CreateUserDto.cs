namespace Application.DTOs;

public sealed record CreateUserDto(
    string Email,
    string Password,
    string Role);
