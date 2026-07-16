using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.Property(user => user.Id).HasColumnName("id");
        builder.Property(user => user.UserName).HasColumnName("user_name").HasMaxLength(256);
        builder.Property(user => user.NormalizedUserName).HasColumnName("normalized_user_name").HasMaxLength(256);
        builder.Property(user => user.Email).HasColumnName("email").HasMaxLength(256);
        builder.Property(user => user.NormalizedEmail).HasColumnName("normalized_email").HasMaxLength(256);
        builder.Property(user => user.EmailConfirmed).HasColumnName("email_confirmed");
        builder.Property(user => user.PasswordHash).HasColumnName("password_hash").HasMaxLength(512);
        builder.Property(user => user.SecurityStamp).HasColumnName("security_stamp");
        builder.Property(user => user.ConcurrencyStamp).HasColumnName("concurrency_stamp");
        builder.Property(user => user.PhoneNumber).HasColumnName("phone_number");
        builder.Property(user => user.PhoneNumberConfirmed).HasColumnName("phone_number_confirmed");
        builder.Property(user => user.TwoFactorEnabled).HasColumnName("two_factor_enabled");
        builder.Property(user => user.LockoutEnd).HasColumnName("lockout_end");
        builder.Property(user => user.LockoutEnabled).HasColumnName("lockout_enabled");
        builder.Property(user => user.AccessFailedCount).HasColumnName("access_failed_count");

        builder.Property(user => user.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
