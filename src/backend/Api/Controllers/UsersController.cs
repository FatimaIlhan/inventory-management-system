using Api.DTOs;
using Api.DTOs.Users;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "AdminOnly")]
public sealed class UsersController(IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AuthenticatedUserDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var createdUser = await authenticationService.CreateUserAsync(
            new CreateUserDto(request.Email, request.Password, request.Role),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<AuthenticatedUserDto>.Ok(createdUser, "User created successfully."));
    }
}
