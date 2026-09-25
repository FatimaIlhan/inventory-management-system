using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authentication;

public enum AdminProvisioningResult
{
    Created,
    SkippedExistingUsers
}

public sealed class AdminProvisioningService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IOptions<SeedAdminOptions> seedAdminOptions,
    ILogger<AdminProvisioningService> logger)
{
    public async Task<AdminProvisioningResult> ProvisionAsync(CancellationToken cancellationToken)
    {
        foreach (var roleName in UserRole.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var createRoleResult = await roleManager.CreateAsync(new Role
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            });

            if (!createRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create the {roleName} role: {FormatErrors(createRoleResult)}");
            }
        }

        if (await userManager.Users.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Administrator provisioning skipped because one or more users already exist.");
            return AdminProvisioningResult.SkippedExistingUsers;
        }

        var adminEmail = seedAdminOptions.Value.Email.Trim().ToLowerInvariant();
        var adminPassword = seedAdminOptions.Value.Password;

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "No users exist, but SeedAdmin:Email and SeedAdmin:Password must be configured to provision the initial administrator.");
        }

        var adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createUserResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create the initial administrator: {FormatErrors(createUserResult)}");
        }

        var addToRoleResult = await userManager.AddToRoleAsync(adminUser, UserRole.Admin);
        if (!addToRoleResult.Succeeded)
        {
            var deleteUserResult = await userManager.DeleteAsync(adminUser);
            var cleanupFailure = deleteUserResult.Succeeded
                ? string.Empty
                : $" The user cleanup also failed: {FormatErrors(deleteUserResult)}";

            throw new InvalidOperationException(
                $"Failed to assign the initial administrator role: {FormatErrors(addToRoleResult)}.{cleanupFailure}");
        }

        logger.LogInformation("Provisioned initial administrator user with email {AdminEmail}.", adminEmail);
        return AdminProvisioningResult.Created;
    }

    private static string FormatErrors(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(error => error.Description));
}