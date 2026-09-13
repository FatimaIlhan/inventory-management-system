
using System.Security.Claims;
using Application.Exceptions;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;


namespace Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public long UserId
    {
        get
        {
            var userIdValue = httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!long.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedException("Current user is not authenticated.");
            }

            return userId;
        }
    }
}

