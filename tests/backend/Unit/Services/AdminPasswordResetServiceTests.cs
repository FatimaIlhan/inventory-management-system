using Domain.Entities;
using Domain.Enums;
using Infrastructure.Authentication;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Unit.Services;

public sealed class AdminPasswordResetServiceTests
{
    private const string Email = "fatima.admin@example.com";
    private const string OldPassword = "Previous123";
    private const string NewPassword = "Replacement456";

    [Theory]
    [InlineData(Email)]
    [InlineData("  FATIMA.ADMIN@EXAMPLE.COM  ")]
    public async Task ResetAsync_ShouldResetOnlyPassword_AndInvalidateExistingTokens(string email)
    {
        await using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var resetter = scope.ServiceProvider.GetRequiredService<AdminPasswordResetService>();
        var user = await CreateUserAsync(manager);
        Assert.True((await roles.CreateAsync(new Role { Name = UserRole.Admin })).Succeeded);
        Assert.True((await manager.AddToRoleAsync(user, UserRole.Admin)).Succeeded);
        var originalStamp = user.SecurityStamp;
        var originalCreatedAt = user.CreatedAtUtc;
        var previousToken = await manager.GeneratePasswordResetTokenAsync(user);

        Assert.True(await resetter.ResetAsync(email, NewPassword));

        Assert.True(await manager.CheckPasswordAsync(user, NewPassword));
        Assert.False(await manager.CheckPasswordAsync(user, OldPassword));
        Assert.NotEqual(originalStamp, user.SecurityStamp);
        Assert.Equal(originalCreatedAt, user.CreatedAtUtc);
        Assert.Equal(Email, user.Email);
        Assert.True(user.EmailConfirmed);
        Assert.Equal(new[] { UserRole.Admin }, await manager.GetRolesAsync(user));
        Assert.Equal(user.Id, (await manager.Users.SingleAsync()).Id);
        var reuseResult = await manager.ResetPasswordAsync(user, previousToken, "Another789");
        Assert.False(reuseResult.Succeeded);
        Assert.Contains(reuseResult.Errors, error => error.Code == "InvalidToken");
        var logs = provider.GetRequiredService<ResetLogger>().Messages;
        Assert.Contains(logs, message => message.Contains("completed successfully"));
        Assert.DoesNotContain(logs, message =>
            message.Contains(NewPassword) || message.Contains(previousToken));
    }

    [Theory]
    [InlineData(null, NewPassword)]
    [InlineData("", NewPassword)]
    [InlineData(" ", NewPassword)]
    [InlineData(Email, null)]
    [InlineData(Email, "")]
    [InlineData(Email, " ")]
    [InlineData("other@example.com", NewPassword)]
    public async Task ResetAsync_ShouldRejectMissingCredentialsOrDifferentEmail(string? email, string? password)
    {
        await using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await CreateUserAsync(manager);
        var stamp = user.SecurityStamp;

        Assert.False(await scope.ServiceProvider.GetRequiredService<AdminPasswordResetService>()
            .ResetAsync(email, password));

        Assert.True(await manager.CheckPasswordAsync(user, OldPassword));
        Assert.Equal(stamp, user.SecurityStamp);
        Assert.Single(await manager.Users.ToListAsync());
        Assert.NotEmpty(provider.GetRequiredService<ResetLogger>().Messages);
    }

    [Fact]
    public async Task ResetAsync_ShouldFailWithoutCreatingUser_WhenTargetDoesNotExist()
    {
        await using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();

        Assert.False(await scope.ServiceProvider.GetRequiredService<AdminPasswordResetService>()
            .ResetAsync(Email, NewPassword));

        Assert.Empty(await scope.ServiceProvider.GetRequiredService<UserManager<User>>().Users.ToListAsync());
        Assert.Contains(provider.GetRequiredService<ResetLogger>().Messages,
            message => message.Contains("does not exist"));
    }

    [Fact]
    public async Task ResetAsync_ShouldPreservePassword_WhenIdentityRejectsWeakPassword()
    {
        await using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await CreateUserAsync(manager);
        var stamp = user.SecurityStamp;

        Assert.False(await scope.ServiceProvider.GetRequiredService<AdminPasswordResetService>()
            .ResetAsync(Email, "weak"));

        Assert.True(await manager.CheckPasswordAsync(user, OldPassword));
        Assert.Equal(stamp, user.SecurityStamp);
        Assert.Contains(provider.GetRequiredService<ResetLogger>().Messages,
            message => message.Contains("minimum length"));
    }

    [Fact]
    public async Task ResetAsync_ShouldNotLogCustomIdentityErrorSecrets()
    {
        await using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        await CreateUserAsync(manager);
        manager.PasswordValidators.Add(new SecretEchoingValidator());

        Assert.False(await scope.ServiceProvider.GetRequiredService<AdminPasswordResetService>()
            .ResetAsync(Email, NewPassword));

        var logs = provider.GetRequiredService<ResetLogger>().Messages;
        Assert.Contains(logs, message => message.Contains("Identity rejected"));
        Assert.DoesNotContain(logs, message => message.Contains(NewPassword));
    }

    private static async Task<User> CreateUserAsync(UserManager<User> manager)
    {
        var user = new User
        {
            UserName = Email,
            Email = Email,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        Assert.True((await manager.CreateAsync(user, OldPassword)).Succeeded);
        return user;
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddDataProtection();
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
            .AddEntityFrameworkStores<InventoryDbContext>()
            .AddDefaultTokenProviders();
        services.AddSingleton<ResetLogger>();
        services.AddSingleton<ILogger<AdminPasswordResetService>>(provider =>
            provider.GetRequiredService<ResetLogger>());
        services.AddScoped<AdminPasswordResetService>();
        return services.BuildServiceProvider();
    }

    private sealed class SecretEchoingValidator : IPasswordValidator<User>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user, string? password) =>
            Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = password!,
                Description = $"Rejected secret: {password}"
            }));
    }

    private sealed class ResetLogger : ILogger<AdminPasswordResetService>
    {
        public List<string> Messages { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter) =>
            Messages.Add(formatter(state, exception));
    }
}
