using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authentication;

public sealed class AdminPasswordResetService(
    UserManager<User> userManager,
    ILogger<AdminPasswordResetService> logger)
{
    public const string EmailEnvironmentVariable = "ADMIN_PASSWORD_RESET_EMAIL";
    public const string PasswordEnvironmentVariable = "ADMIN_PASSWORD_RESET_NEW_PASSWORD";
    private const string AllowedEmail = "fatima.admin@example.com";

    public async Task<bool> ResetAsync(string? email, string? newPassword)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword))
        {
            logger.LogError(
                "Administrator password reset requires both {EmailVariable} and {PasswordVariable}.",
                EmailEnvironmentVariable, PasswordEnvironmentVariable);
            return false;
        }

        if (!string.Equals(email.Trim(), AllowedEmail, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogError("Administrator password reset is restricted to the configured target administrator.");
            return false;
        }

        // FindByEmailAsync applies Identity's configured email normalizer.
        var user = await userManager.FindByEmailAsync(AllowedEmail);
        if (user is null)
        {
            logger.LogError("Administrator password reset failed because the target user does not exist.");
            return false;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                logger.LogError("Administrator password reset rejected: {Description}.", SafeDescription(error));
            }

            return false;
        }

        logger.LogInformation("Administrator password reset completed successfully.");
        return true;
    }

    private static string SafeDescription(IdentityError error)
    {
        // Never emit arbitrary validator text: a custom validator could include the supplied password.
        var describer = new IdentityErrorDescriber();
        return error.Code switch
        {
            nameof(IdentityErrorDescriber.PasswordTooShort) => "The password does not meet the minimum length requirement",
            nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) => "The password does not contain enough unique characters",
            nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric) => describer.PasswordRequiresNonAlphanumeric().Description,
            nameof(IdentityErrorDescriber.PasswordRequiresDigit) => describer.PasswordRequiresDigit().Description,
            nameof(IdentityErrorDescriber.PasswordRequiresLower) => describer.PasswordRequiresLower().Description,
            nameof(IdentityErrorDescriber.PasswordRequiresUpper) => describer.PasswordRequiresUpper().Description,
            nameof(IdentityErrorDescriber.InvalidToken) => describer.InvalidToken().Description,
            nameof(IdentityErrorDescriber.ConcurrencyFailure) => describer.ConcurrencyFailure().Description,
            _ => "Identity rejected the password reset"
        };
    }
}
