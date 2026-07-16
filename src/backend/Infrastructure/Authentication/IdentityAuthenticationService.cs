using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authentication;

public sealed class IdentityAuthenticationService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    InventoryDbContext dbContext,
    IJwtTokenService jwtTokenService,
    IRefreshTokenGenerator refreshTokenGenerator,
    TimeProvider timeProvider) : IAuthenticationService
{
    private static readonly HashSet<string> AllowedRoles =
        [UserRole.Admin, UserRole.Manager, UserRole.Employee];

    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    public async Task<AuthResultDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(loginDto.Email);
        var user = await userManager.Users.FirstOrDefaultAsync(item => item.NormalizedEmail == normalizedEmail.ToUpperInvariant(), cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!passwordValid)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var roleName = await ResolvePrimaryRoleAsync(user);
        return await IssueTokensAsync(user, roleName, cancellationToken);
    }

    public async Task<AuthResultDto> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new AppValidationException("Refresh token is required.");
        }

        var storedToken = await dbContext.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.Token == refreshToken, cancellationToken);

        if (storedToken is null || storedToken.User is null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        if (!storedToken.IsActive)
        {
            throw new UnauthorizedException("Refresh token is no longer active.");
        }

        storedToken.RevokedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        await dbContext.SaveChangesAsync(cancellationToken);

        var roleName = await ResolvePrimaryRoleAsync(storedToken.User);
        return await IssueTokensAsync(storedToken.User, roleName, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new AppValidationException("Refresh token is required.");
        }

        var storedToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(token => token.Token == refreshToken, cancellationToken);
        if (storedToken is null)
        {
            return;
        }

        storedToken.RevokedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthenticatedUserDto> GetCurrentUserAsync(long userId, CancellationToken cancellationToken)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User was not found.");
        }

        var roleName = await ResolvePrimaryRoleAsync(user);
        return new AuthenticatedUserDto(user.Id, user.Email ?? string.Empty, roleName);
    }

    public async Task<AuthenticatedUserDto> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(createUserDto.Email))
        {
            throw new AppValidationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(createUserDto.Password) || createUserDto.Password.Length < 8)
        {
            throw new AppValidationException("Password must be at least 8 characters.");
        }

        if (string.IsNullOrWhiteSpace(createUserDto.Role) || !AllowedRoles.Contains(createUserDto.Role))
        {
            throw new AppValidationException("Role must be Admin, Manager, or Employee.");
        }

        var normalizedEmail = NormalizeEmail(createUserDto.Email);
        var existingUser = await userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            throw new AppValidationException("A user with this email already exists.");
        }

        var roleExists = await roleManager.RoleExistsAsync(createUserDto.Role);
        if (!roleExists)
        {
            throw new AppValidationException("Role does not exist in database.");
        }

        var user = new User
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, createUserDto.Password);
        if (!createResult.Succeeded)
        {
            var identityErrors = string.Join("; ", createResult.Errors.Select(error => error.Description));
            throw new AppValidationException(string.IsNullOrWhiteSpace(identityErrors) ? "Failed to create user." : identityErrors);
        }

        var roleAssignmentResult = await userManager.AddToRoleAsync(user, createUserDto.Role);
        if (!roleAssignmentResult.Succeeded)
        {
            var identityErrors = string.Join("; ", roleAssignmentResult.Errors.Select(error => error.Description));
            throw new AppValidationException(string.IsNullOrWhiteSpace(identityErrors) ? "Failed to assign user role." : identityErrors);
        }

        return new AuthenticatedUserDto(user.Id, user.Email ?? string.Empty, createUserDto.Role);
    }

    private async Task<AuthResultDto> IssueTokensAsync(User user, string roleName, CancellationToken cancellationToken)
    {
        var accessToken = jwtTokenService.GenerateAccessToken(user, roleName);
        var refreshToken = refreshTokenGenerator.GenerateToken();
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        await dbContext.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc.Add(RefreshTokenLifetime)
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(
            accessToken,
            refreshToken,
            jwtTokenService.AccessTokenLifetimeSeconds,
            new AuthenticatedUserDto(user.Id, user.Email ?? string.Empty, roleName));
    }

    private async Task<string> ResolvePrimaryRoleAsync(User user)
    {
        var assignedRoles = await userManager.GetRolesAsync(user);
        var roleName = assignedRoles.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new UnauthorizedException("User is missing a role assignment.");
        }

        return roleName;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
