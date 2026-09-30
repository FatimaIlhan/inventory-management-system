using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authentication;

public sealed class JwtOptionsValidator(
    IConfiguration configuration,
    IHostEnvironment hostEnvironment) : IValidateOptions<JwtOptions>
{
    public const int MinimumSigningKeyBytes = 32;
    public const int MinimumAccessTokenLifetimeMinutes = 1;
    public const int MaximumAccessTokenLifetimeMinutes = 60;

    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.SigningKey))
        {
            failures.Add("Jwt:SigningKey must be configured via user secrets or environment variables.");
        }
        else if (hostEnvironment.IsProduction() && Encoding.UTF8.GetByteCount(options.SigningKey) < MinimumSigningKeyBytes)
        {
            failures.Add($"Jwt:SigningKey must be at least {MinimumSigningKeyBytes} UTF-8 bytes in Production.");
        }

        if (hostEnvironment.IsProduction())
        {
            ValidateRequiredValue(options.Issuer, "Jwt:Issuer", failures);
            ValidateRequiredValue(options.Audience, "Jwt:Audience", failures);
            ValidateAccessTokenLifetime(options, failures);
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private void ValidateAccessTokenLifetime(JwtOptions options, List<string> failures)
    {
        var configuredValue = configuration[$"{JwtOptions.SectionName}:AccessTokenLifetimeMinutes"];

        if (string.IsNullOrWhiteSpace(configuredValue) || !int.TryParse(configuredValue, out _))
        {
            failures.Add("Jwt:AccessTokenLifetimeMinutes must be configured as an integer in Production.");
            return;
        }

        if (options.AccessTokenLifetimeMinutes < MinimumAccessTokenLifetimeMinutes ||
            options.AccessTokenLifetimeMinutes > MaximumAccessTokenLifetimeMinutes)
        {
            failures.Add($"Jwt:AccessTokenLifetimeMinutes must be between {MinimumAccessTokenLifetimeMinutes} and {MaximumAccessTokenLifetimeMinutes} in Production.");
        }
    }

    private static void ValidateRequiredValue(string value, string configurationKey, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add($"{configurationKey} must be configured in Production.");
        }
    }
}