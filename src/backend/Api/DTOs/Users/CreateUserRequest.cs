namespace Api.DTOs.Users;

public sealed record CreateUserRequest(
    string Email,
    string Password,
    string Role);
