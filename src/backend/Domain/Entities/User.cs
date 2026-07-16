using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public sealed class User : IdentityUser<long>
{
    public DateTime CreatedAtUtc { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
