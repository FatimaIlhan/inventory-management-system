using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(token => token.Id);

        builder.Property(token => token.Id)
            .HasColumnName("id");

        builder.Property(token => token.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(token => token.Token)
            .HasColumnName("token")
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(token => token.Token)
            .IsUnique();

        builder.Property(token => token.ExpiresAtUtc)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(token => token.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(token => token.RevokedAtUtc)
            .HasColumnName("revoked_at");

        builder.HasOne(token => token.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
