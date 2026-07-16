namespace Application.DTOs;

public sealed record AuthResultDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    AuthenticatedUserDto User);
