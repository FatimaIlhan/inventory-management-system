using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authentication;

public sealed class AuthSeedHostedService(
    IServiceProvider serviceProvider,
    IOptions<SeedAdminOptions> seedAdminOptions,
    ILogger<AuthSeedHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        foreach (var roleName in UserRole.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Role
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                });
            }
        }

        if (userManager.Users.Any())
        {
            return;
        }

        var adminEmail = seedAdminOptions.Value.Email.Trim().ToLowerInvariant();
        var adminPassword = seedAdminOptions.Value.Password;

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("No users found but SeedAdmin credentials are not configured. Configure SeedAdmin:Email and SeedAdmin:Password to bootstrap an admin user.");
            return;
        }

        var adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createResult.Succeeded)
        {
            logger.LogError("Failed to seed admin user: {Errors}", string.Join("; ", createResult.Errors.Select(error => error.Description)));
            return;
        }

        var roleResult = await userManager.AddToRoleAsync(adminUser, UserRole.Admin);
        if (!roleResult.Succeeded)
        {
            logger.LogError("Failed to assign admin role while seeding user: {Errors}", string.Join("; ", roleResult.Errors.Select(error => error.Description)));
            return;
        }

        logger.LogInformation("Seeded initial admin user with email {AdminEmail}.", adminEmail);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
