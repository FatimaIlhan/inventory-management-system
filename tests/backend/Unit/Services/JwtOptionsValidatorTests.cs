using Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Unit.Services;

public sealed class JwtOptionsValidatorTests
{
    [Fact]
    public void Validate_ShouldFail_WhenSigningKeyIsMissing()
    {
        var result = ValidateProduction(CreateValidOptions(signingKey: string.Empty));

        AssertFailedWith(result, "Jwt:SigningKey");
    }

    [Fact]
    public void Validate_ShouldFail_WhenSigningKeyIsBlank()
    {
        var result = ValidateProduction(CreateValidOptions(signingKey: "   "));

        AssertFailedWith(result, "Jwt:SigningKey");
    }

    [Fact]
    public void Validate_ShouldFail_WhenSigningKeyIsShorterThanThirtyTwoUtf8Bytes()
    {
        var result = ValidateProduction(CreateValidOptions(
            signingKey: new string('a', JwtOptionsValidator.MinimumSigningKeyBytes - 1)));

        AssertFailedWith(result, "Jwt:SigningKey");
    }

    [Fact]
    public void Validate_ShouldMeasureSigningKeyLengthInUtf8Bytes()
    {
        var result = ValidateProduction(CreateValidOptions(
            signingKey: new string('\u00e9', JwtOptionsValidator.MinimumSigningKeyBytes / 2)));

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenSigningKeyIsValid()
    {
        var result = ValidateProduction(CreateValidOptions());

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenIssuerIsMissingOrBlank(string? issuer)
    {
        var result = ValidateProduction(CreateValidOptions(issuer: issuer ?? string.Empty));

        AssertFailedWith(result, "Jwt:Issuer");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenIssuerIsValid()
    {
        var result = ValidateProduction(CreateValidOptions());

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenAudienceIsMissingOrBlank(string? audience)
    {
        var result = ValidateProduction(CreateValidOptions(audience: audience ?? string.Empty));

        AssertFailedWith(result, "Jwt:Audience");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenAudienceIsValid()
    {
        var result = ValidateProduction(CreateValidOptions());

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(61)]
    public void Validate_ShouldFail_WhenLifetimeIsOutsideProductionRange(int lifetimeMinutes)
    {
        var result = ValidateProduction(CreateValidOptions(lifetimeMinutes: lifetimeMinutes), lifetimeMinutes.ToString());

        AssertFailedWith(result, "Jwt:AccessTokenLifetimeMinutes");
    }

    [Fact]
    public void Validate_ShouldFail_WhenLifetimeIsNonNumeric()
    {
        var result = ValidateProduction(CreateValidOptions(), "invalid");

        AssertFailedWith(result, "Jwt:AccessTokenLifetimeMinutes");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenLifetimeIsValid()
    {
        var result = ValidateProduction(CreateValidOptions(), "15");

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_ShouldSucceed_ForCompleteValidProductionConfiguration()
    {
        var result = ValidateProduction(CreateValidOptions());

        Assert.True(result.Succeeded);
    }

    private static JwtOptions CreateValidOptions(
        string? issuer = null,
        string? audience = null,
        string? signingKey = null,
        int lifetimeMinutes = 15) => new()
        {
            Issuer = issuer ?? "inventory-management-system",
            Audience = audience ?? "inventory-management-system-client",
            SigningKey = signingKey ?? new string('a', JwtOptionsValidator.MinimumSigningKeyBytes),
            AccessTokenLifetimeMinutes = lifetimeMinutes
        };

    private static ValidateOptionsResult ValidateProduction(JwtOptions options, string? configuredLifetime = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{JwtOptions.SectionName}:AccessTokenLifetimeMinutes"] = configuredLifetime ?? options.AccessTokenLifetimeMinutes.ToString()
            })
            .Build();
        var validator = new JwtOptionsValidator(configuration, new TestHostEnvironment { EnvironmentName = Environments.Production });

        return validator.Validate(null, options);
    }

    private static void AssertFailedWith(ValidateOptionsResult result, string configurationKey)
    {
        Assert.False(result.Succeeded);
        Assert.Contains(result.Failures!, failure => failure.Contains(configurationKey, StringComparison.Ordinal));
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = "UnitTests";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}