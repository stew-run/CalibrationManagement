using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;
using CalibrationManagement.Api.Configuration;
using CalibrationManagement.Domain.Constants;
using CalibrationManagement.Domain.Entities;
using CalibrationManagement.Infrastructure.Security;

namespace CalibrationManagement.Tests;

public class PasswordServiceTests
{
    private readonly PasswordService _service = new(workFactor: 4); // low cost keeps the tests fast

    [Fact]
    public void Hash_then_verify_round_trips_and_never_returns_the_plain_password()
    {
        var hash = _service.Hash("Correct-Horse-Battery-9");
        Assert.NotEqual("Correct-Horse-Battery-9", hash);
        Assert.StartsWith("$2", hash);
        Assert.True(_service.Verify("Correct-Horse-Battery-9", hash));
        Assert.False(_service.Verify("wrong-password", hash));
    }

    [Fact]
    public void Verify_with_missing_or_malformed_hash_is_false()
    {
        Assert.False(_service.Verify("anything", null));
        Assert.False(_service.Verify("anything", "not-a-bcrypt-hash"));
    }

    [Theory]
    [InlineData("short1A")]
    [InlineData("alllowercase12345")]
    [InlineData("ALLUPPERCASE12345")]
    [InlineData("NoDigitsInHereAtAll")]
    [InlineData("MyPassword12345")]
    public void Weak_passwords_are_rejected(string password)
        => Assert.NotEmpty(_service.Validate(password));

    [Fact]
    public void Strong_password_is_accepted_and_username_is_not_allowed_inside_it()
    {
        Assert.Empty(_service.Validate("Tr1cky-Calibr8-Gauge"));
        Assert.NotEmpty(_service.Validate("Tr1cky-Calibr8-Gauge", "calibr8"));
    }
}

public class JwtTokenServiceTests
{
    private static JwtSettings Settings(string key = "unit-test-signing-key-that-is-long-enough-1234567890")
        => new() { Key = key, Issuer = "issuer", Audience = "audience", ExpiryMinutes = 30 };

    private static User User() => new() { UserId = 7, Username = "tech1", Role = RoleNames.Technician, Email = "t@x.test", FullName = "Tech One", IsActive = true };

    [Fact]
    public void Token_is_short_lived_signed_and_carries_id_and_role()
    {
        var settings = Settings();
        var (token, expires) = new JwtTokenService(Options.Create(settings)).CreateToken(User());

        Assert.InRange(expires, DateTime.UtcNow.AddMinutes(29), DateTime.UtcNow.AddMinutes(31));
        var principal = new JwtSecurityTokenHandler().ValidateToken(token, JwtTokenService.BuildValidationParameters(settings), out _);
        Assert.Equal("7", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.True(principal.IsInRole(RoleNames.Technician));
        Assert.False(principal.IsInRole(RoleNames.Administrator));
    }

    [Fact]
    public void Token_signed_with_another_key_is_rejected()
    {
        var (token, _) = new JwtTokenService(Options.Create(Settings())).CreateToken(User());
        var other = Settings("a-completely-different-signing-key-of-sufficient-length-99");
        Assert.ThrowsAny<SecurityTokenException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(token, JwtTokenService.BuildValidationParameters(other), out _));
    }
}

public class ConfigurationValidatorTests
{
    [Fact]
    public void Missing_connection_string_and_short_key_are_reported()
    {
        var problems = ConfigurationValidator.Validate("", new JwtSettings { Key = "short" }, requireJwt: true);
        Assert.Equal(2, problems.Count);
    }

    [Fact]
    public void Placeholder_key_is_rejected()
    {
        var problems = ConfigurationValidator.Validate("Server=.;Database=x", new JwtSettings { Key = "PASTE-A-RANDOM-SECRET-OF-AT-LEAST-32-CHARACTERS-HERE" }, true);
        Assert.Single(problems);
    }

    [Fact]
    public void Valid_configuration_has_no_problems_and_provisioning_does_not_need_a_jwt_key()
    {
        Assert.Empty(ConfigurationValidator.Validate("Server=.;Database=x", new JwtSettings { Key = new string('k', 40) }, true));
        Assert.Empty(ConfigurationValidator.Validate("Server=.;Database=x", new JwtSettings(), requireJwt: false));
    }
}
