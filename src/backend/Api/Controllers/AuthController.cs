using System.Security.Claims;
using Api.DTOs;
using Api.DTOs.Auth;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var authResult = await authenticationService.LoginAsync(
            new LoginDto(request.Email, request.Password),
            cancellationToken);

        return Ok(ApiResponse<AuthResultDto>.Ok(authResult, "Login successful."));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var authResult = await authenticationService.RefreshAsync(request.RefreshToken, cancellationToken);
        return Ok(ApiResponse<AuthResultDto>.Ok(authResult, "Token refreshed."));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await authenticationService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AuthenticatedUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid user identity token."));
        }

        var user = await authenticationService.GetCurrentUserAsync(userId, cancellationToken);
        return Ok(ApiResponse<AuthenticatedUserDto>.Ok(user));
    }
}
