using Application.DTOs;

namespace Application.Interfaces;

public interface IAuthenticationService
{
    Task<AuthResultDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken);
    Task<AuthResultDto> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);
    Task<AuthenticatedUserDto> GetCurrentUserAsync(long userId, CancellationToken cancellationToken);
    Task<AuthenticatedUserDto> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken);
}
