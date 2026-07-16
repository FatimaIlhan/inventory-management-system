using System.Security.Cryptography;
using Application.Interfaces;

namespace Infrastructure.Authentication;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
