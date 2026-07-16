using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

public sealed class JwtTokenService(IOptions<JwtOptions> jwtOptions, TimeProvider timeProvider) : IJwtTokenService
{
    private readonly JwtOptions options = jwtOptions.Value;

    public int AccessTokenLifetimeSeconds => options.AccessTokenLifetimeMinutes * 60;

    public string GenerateAccessToken(User user, string roleName)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, roleName)
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: nowUtc,
            expires: nowUtc.AddMinutes(options.AccessTokenLifetimeMinutes),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
