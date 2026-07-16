using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.Property(role => role.Id).HasColumnName("id");
        builder.Property(role => role.Name).HasColumnName("name").HasMaxLength(50);
        builder.Property(role => role.NormalizedName).HasColumnName("normalized_name").HasMaxLength(50);
        builder.Property(role => role.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        builder.HasData(
            new Role { Id = 1, Name = UserRole.Admin, NormalizedName = UserRole.Admin.ToUpperInvariant() },
            new Role { Id = 2, Name = UserRole.Manager, NormalizedName = UserRole.Manager.ToUpperInvariant() },
            new Role { Id = 3, Name = UserRole.Employee, NormalizedName = UserRole.Employee.ToUpperInvariant() });
    }
}
