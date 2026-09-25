using Domain.Entities;
using Domain.Enums;
using Infrastructure.Authentication;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Unit.Services;

public sealed class AdminProvisioningServiceTests
{
    [Fact]
    public async Task ProvisionAsync_ShouldCreateInitialAdministrator_WhenNoUsersExist()
    {
        await using var serviceProvider = CreateServiceProvider("admin@example.com", "Admin123");
        using var scope = serviceProvider.CreateScope();
        var provisioner = scope.ServiceProvider.GetRequiredService<AdminProvisioningService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var result = await provisioner.ProvisionAsync(CancellationToken.None);

        var administrator = await userManager.FindByEmailAsync("admin@example.com");
        Assert.Equal(AdminProvisioningResult.Created, result);
        Assert.NotNull(administrator);
        Assert.True(await userManager.IsInRoleAsync(administrator, UserRole.Admin));
    }

    [Fact]
    public async Task ProvisionAsync_ShouldNotCreateAdministrator_WhenUsersAlreadyExist()
    {
        await using var serviceProvider = CreateServiceProvider("admin@example.com", "Admin123");
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var provisioner = scope.ServiceProvider.GetRequiredService<AdminProvisioningService>();
        var existingUser = new User
        {
            UserName = "existing@example.com",
            Email = "existing@example.com",
            CreatedAtUtc = DateTime.UtcNow
        };

        Assert.True((await userManager.CreateAsync(existingUser, "Existing123")).Succeeded);

        var result = await provisioner.ProvisionAsync(CancellationToken.None);

        Assert.Equal(AdminProvisioningResult.SkippedExistingUsers, result);
        Assert.Null(await userManager.FindByEmailAsync("admin@example.com"));
    }

    [Fact]
    public async Task ProvisionAsync_ShouldFail_WhenNoUsersExistAndCredentialsAreMissing()
    {
        await using var serviceProvider = CreateServiceProvider(string.Empty, string.Empty);
        using var scope = serviceProvider.CreateScope();
        var provisioner = scope.ServiceProvider.GetRequiredService<AdminProvisioningService>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => provisioner.ProvisionAsync(CancellationToken.None));

        Assert.Equal(
            "No users exist, but SeedAdmin:Email and SeedAdmin:Password must be configured to provision the initial administrator.",
            exception.Message);
    }

    private static ServiceProvider CreateServiceProvider(string email, string password)
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<InventoryDbContext>();
        services.AddSingleton<IOptions<SeedAdminOptions>>(
            Options.Create(new SeedAdminOptions
            {
                Email = email,
                Password = password
            }));
        services.AddScoped<AdminProvisioningService>();

        return services.BuildServiceProvider();
    }
}